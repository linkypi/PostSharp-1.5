using PostSharp.Samples.Silverlight.Aspects;

namespace PostSharp.Samples.Silverlight.Test
{
    [NotifyPropertyChanged]
    public class Person
    {
        [Required] private string firstName;
        [Required] private string lastName;

        public Person( string firstName, string lastName, string country )
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.Country = country;
        }
       
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string Country
        {
            get; private set;
        }

    }
}