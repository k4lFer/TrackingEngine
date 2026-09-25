namespace App.Shared.Utils
{
    public static class ExceptionMessageHelper
    {
        public static string GetDeepestMessage(Exception ex)
        {
            var current = ex;
            while (current.InnerException != null)
                current = current.InnerException;

            return current.Message;
        }
    }
}