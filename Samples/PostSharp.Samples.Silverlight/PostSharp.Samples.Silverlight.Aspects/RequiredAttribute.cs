using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PostSharp.Laos;

namespace PostSharp.Samples.Silverlight.Aspects
{
    public class RequiredAttribute : OnFieldAccessAspect
    {

        public override void OnSetValue(FieldAccessEventArgs eventArgs)
        {
            string s;
            if (eventArgs.ExposedFieldValue == null ||
                (
                (s = eventArgs.ExposedFieldValue as string) != null)
                && s.Length == 0)
            {
                throw new ArgumentNullException(
                    eventArgs.FieldInfo.Name,
                    "Cannot set this field to null or empty.");
            }
            base.OnSetValue(eventArgs);
        }

    }
}
