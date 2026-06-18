using System.Collections.Generic;

namespace Infrastructure.AI.Models;

internal record HeyGenSubmitRes(HeyGenSubmitData? Data);
internal record HeyGenSubmitData(string? VideoId);

internal record HeyGenStatusRes(HeyGenStatusData? Data);
internal record HeyGenStatusData(string? Status, string? VideoUrl, HeyGenError? Error);
internal record HeyGenError(int Code, string? Message);

internal record HeyGenAvatarsRes(HeyGenAvatarsData? Data);
internal record HeyGenAvatarsData(List<HeyGenAvatarItem>? Avatars);
internal record HeyGenAvatarItem(
    string? AvatarId,
    string? AvatarName,
    string? PreviewImageUrl,
    string? PreviewVideoUrl,
    string? Gender
);
