namespace UserManagement.Api.Constants
{
    public static class Messages
    {
        public static class Error
        {
            public const string Unexpected = "An unexpected error occurred on the server.";
            public const string UserNotFound = "User with id {0} was not found.";
            public const string UserCreationFailed = "User creation failed.";
            public const string ValidationFailed = "Validation failed.";
            public const string InvalidCredentials = "Invalid email or password.";
            public const string EmailNotConfirmed = "Please confirm your email address before logging in.";
            public const string ConfirmRegisterFailed = "Registration confirmation failed.";
            public const string EmailAlreadyExists = "Email address is already in use.";
        }

        public static class Success
        {
            public const string UserCreated = "User created successfully.";
            public const string UserUpdated = "User updated successfully.";
            public const string UserDeleted = "User deleted successfully.";
            public const string RequestSuccessful = "Request successful";
            public const string LoginSuccessful = "Login successful.";
            public const string RegistrationConfirmed = "Registration confirmed and password set successfully.";
        }
    }
}
