namespace JobTrackr.Application.Common
{
    public static class ErrorMessages
    {
        public const string UserNotFound = "User not found.";
        public const string TaskNotFound = "Task not found.";
        public const string TaskTitleRequired = "Task title is required.";
        public const string TaskPriorityRequired = "Task priority is required.";
        public const string TaskPriorityInvalid =
            "Task priority must be Low, Medium, or High.";
        public const string TaskDueDateMustBeFuture =
            "Task due date must be in the future.";
        public const string UserIdRequired = "UserId is required.";
        public const string UserFullNameRequired = "User full name is required.";
        public const string UserEmailRequired = "User email is required.";
        public const string EmailInvalid = "Email is not valid.";
        public const string EmailAlreadyInUse = "Email is already in use.";
        public const string CurrentPasswordIncorrect = "Current password is incorrect.";
        public const string NewPasswordMismatch = "New password and confirmation do not match.";
        public const string NewPasswordMustBeDifferent =
            "New password must be different from the current password.";
    }
}
