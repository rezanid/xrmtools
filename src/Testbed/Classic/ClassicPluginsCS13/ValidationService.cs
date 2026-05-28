using System;

namespace XrmGenTest;

public interface IValidationService
{
    (bool passed, string message) ValidateContact(Contact contact);
}

public class ValidationService : IValidationService
{
    public void Dispose() => throw new NotImplementedException();
    
    public (bool passed, string message) ValidateContact(Contact contact)
    {
        if (contact == null) throw new ArgumentNullException(nameof(contact));
        if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
        {
            return (false, "Firstname and / or lastname missing");
        }
        return (true, null);
    }
}