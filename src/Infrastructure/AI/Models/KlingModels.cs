using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Infrastructure.AI.Models;

internal record KlingTaskRes(
    [property: JsonPropertyName("data")] KlingTaskData? Data
);

internal record KlingTaskData(
    [property: JsonPropertyName("task_id")] string? TaskId
);

internal record KlingDetailRes(
    [property: JsonPropertyName("data")] KlingDetailData? Data
);

internal record KlingDetailData(
    [property: JsonPropertyName("task_status")] string? TaskStatus,
    [property: JsonPropertyName("task_result")] KlingTaskResultData? TaskResult
);

internal record KlingTaskResultData(
    [property: JsonPropertyName("images")] List<KlingImage>? Images,
    [property: JsonPropertyName("videos")] List<KlingVideo>? Videos
);

internal record KlingImage(
    [property: JsonPropertyName("url")] string? Url
);

internal record KlingVideo(
    [property: JsonPropertyName("url")] string? Url
);
