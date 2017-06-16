using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;

namespace PostSharp.Samples.Micro
{
    // This class dispatches input events from emulated GPIO pins (0-4) to Input.Button 
    // events. It is specific to the SDK's sample emulator; if you use this code,
    // please update this class to reflect the design of your hardware.
    public sealed class GPIOButtonInputProvider
    {
        public readonly Dispatcher Dispatcher;

        private ButtonPad[] buttons;
        private ReportInputCallback callback;
        private InputProviderSite site;
        private PresentationSource source;

        private delegate bool ReportInputCallback(InputReport inputReport);

        // This class maps GPIOs to Buttons processable by Microsoft.SPOT.Presentation
        public GPIOButtonInputProvider(PresentationSource source)
        {
            // Set the input source.
            this.source = source;
            // Register our object as an input source with the input manager and get back an
            // InputProviderSite object which forwards the input report to the input manager,
            // which then places the input in the staging area.
            site = InputManager.CurrentInputManager.RegisterInputProvider(this);
            // Create a delegate that refers to the InputProviderSite object's ReportInput method
            callback = new ReportInputCallback(site.ReportInput);
            Dispatcher = Dispatcher.CurrentDispatcher;

            // Allocate button pads and assign the (emulated) hardware pins as input 
            // from specific buttons.
            ButtonPad[] buttons = new ButtonPad[]
            {
                // Associate the buttons to the pins as setup in the emulator/hardware
                new ButtonPad(this, Button.Left  , Cpu.Pin.GPIO_Pin0),
                new ButtonPad(this, Button.Right , Cpu.Pin.GPIO_Pin1),
                new ButtonPad(this, Button.Up    , Cpu.Pin.GPIO_Pin2),
                new ButtonPad(this, Button.Select, Cpu.Pin.GPIO_Pin3),
                new ButtonPad(this, Button.Down  , Cpu.Pin.GPIO_Pin4),
            };

            this.buttons = buttons;
        }

        // The emulated device provides a button pad containing five buttons 
        // for user input. This class represents the button pad.
        internal class ButtonPad
        {
            private Button button;
            private InterruptPort port;
            private GPIOButtonInputProvider sink;

            // Construct the object. Set this class to handle the emulated 
            // hardware's button interrupts.
            public ButtonPad(GPIOButtonInputProvider sink, Button button, Cpu.Pin pin)
            {
                this.sink = sink;
                this.button = button;

                // When this GPIO pin is true, call the Interrupt method.
                port = new InterruptPort(pin, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth);
                port.OnInterrupt += new GPIOInterruptEventHandler(this.Interrupt);
            }

            void Interrupt(Cpu.Pin port, bool state, TimeSpan time)
            {
                RawButtonActions action = state ? RawButtonActions.ButtonUp : RawButtonActions.ButtonDown;

                RawButtonInputReport report = new RawButtonInputReport(sink.source, time, button, action);

                // Queue the button press to the input provider site.
                sink.Dispatcher.BeginInvoke(sink.callback, report);
            }
        }
    }
}


