namespace Domain.Constants;

public static class JobStatus
{
    // General statuses
    public const string Pending = "pending";
    public const string Scripting = "scripting";
    public const string Done = "done";
    public const string Error = "error";

    // Flow A: Avatar statuses
    public const string Voicing = "voicing";      // ElevenLabs TTS
    public const string Rendering = "rendering";  // HeyGen video render
    public const string Composing = "composing";  // FFmpeg video composition

    // Flow B & C extra statuses
    public const string Scraping = "scraping";    // Scraping product info
    public const string TryingOn = "tryingon";    // Kling Try-on processing
    public const string Animating = "animating";  // Kling Image-to-Video processing
}
