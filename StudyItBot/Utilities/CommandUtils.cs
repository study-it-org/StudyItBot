using Discord;

namespace StudyItBot.Utilities;

public static class CommandUtils
{
    /// <summary>
    /// Delete a message in another thread and ignore every exception
    /// </summary>
    /// <param name="msg">The message to delete</param>
    /// <param name="timeout">time until message gets deleted</param>
    public static void DeleteAndForget(this IMessage msg, int timeout = 0)
    {
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeout);
                await msg.DeleteAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignored
            }
        });
    }
}