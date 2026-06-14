namespace Infrastructure.Persistence;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class SeedData
{
    public static async Task SeedHookTemplatesAsync(AppDbContext db)
    {
        if (await db.HookTemplates.AnyAsync())
            return;

        var hooks = new List<HookTemplate>
        {
            // --- FASHION ---
            new() { Category = "fashion", HookText = "Đừng mua chiếc đầm này nếu bạn không muốn thu hút mọi ánh nhìn!", Language = "vi", PerformanceScore = 0.95m },
            new() { Category = "fashion", HookText = "Unbox chiếc áo thun đang hot rần rần trên TikTok Shop mấy ngày qua.", Language = "vi", PerformanceScore = 0.90m },
            new() { Category = "fashion", HookText = "Cách phối đồ cực đỉnh cho các bạn nữ nấm lùn hack chiều cao.", Language = "vi", PerformanceScore = 0.88m },
            new() { Category = "fashion", HookText = "Chiếc quần jean tôn dáng nhất mà mình từng sở hữu.", Language = "vi", PerformanceScore = 0.85m },
            new() { Category = "fashion", HookText = "Phát hiện một shop bán váy siêu xinh giá hạt dẻ.", Language = "vi", PerformanceScore = 0.87m },
            new() { Category = "fashion", HookText = "Ba cách mặc áo sơ mi không bao giờ lỗi mốt.", Language = "vi", PerformanceScore = 0.80m },
            new() { Category = "fashion", HookText = "Đây là lý do tại sao bộ đồ này lại hot đến thế!", Language = "vi", PerformanceScore = 0.89m },
            new() { Category = "fashion", HookText = "Review chân thực chiếc đầm body siêu hack dáng.", Language = "vi", PerformanceScore = 0.92m },
            new() { Category = "fashion", HookText = "Phối đồ phong cách casual đi chơi mùa hè cực chất.", Language = "vi", PerformanceScore = 0.82m },
            new() { Category = "fashion", HookText = "Cận cảnh chất vải của chiếc áo hoodie đang bán chạy nhất.", Language = "vi", PerformanceScore = 0.86m },

            // --- BEAUTY ---
            new() { Category = "beauty", HookText = "Màu son này chắc chắn sẽ làm bạn bất ngờ!", Language = "vi", PerformanceScore = 0.94m },
            new() { Category = "beauty", HookText = "Bí quyết giữ lớp trang điểm lâu trôi suốt cả ngày.", Language = "vi", PerformanceScore = 0.83m },
            new() { Category = "beauty", HookText = "Đánh giá chi tiết cây son hot hit nhất hiện nay.", Language = "vi", PerformanceScore = 0.91m },
            new() { Category = "beauty", HookText = "Sự thật về dòng cushion đang được review rầm rộ.", Language = "vi", PerformanceScore = 0.89m },
            new() { Category = "beauty", HookText = "Son môi giá rẻ nhưng chất lượng không hề rẻ.", Language = "vi", PerformanceScore = 0.87m },
            new() { Category = "beauty", HookText = "Màu son đỉnh cao dành cho các bạn da ngăm.", Language = "vi", PerformanceScore = 0.86m },
            new() { Category = "beauty", HookText = "Bôi thử màu son này và cái kết bất ngờ.", Language = "vi", PerformanceScore = 0.93m },
            new() { Category = "beauty", HookText = "Cầm trên tay dòng mascara chống trôi đỉnh nhất.", Language = "vi", PerformanceScore = 0.88m },
            new() { Category = "beauty", HookText = "Cách đánh má hồng tự nhiên như các tỷ tỷ xứ Trung.", Language = "vi", PerformanceScore = 0.85m },
            new() { Category = "beauty", HookText = "Review nhanh set trang điểm cá nhân cho người mới bắt đầu.", Language = "vi", PerformanceScore = 0.81m },

            // --- SKINCARE ---
            new() { Category = "skincare", HookText = "Routine dưỡng da tối giản giúp da căng bóng sau 7 ngày.", Language = "vi", PerformanceScore = 0.93m },
            new() { Category = "skincare", HookText = "Đừng dùng serum này nếu da bạn nhạy cảm!", Language = "vi", PerformanceScore = 0.96m },
            new() { Category = "skincare", HookText = "Phát hiện kem chống nắng kiềm dầu cực đỉnh cho mùa hè.", Language = "vi", PerformanceScore = 0.90m },
            new() { Category = "skincare", HookText = "Trải nghiệm dòng sữa rửa mặt dịu nhẹ nhất.", Language = "vi", PerformanceScore = 0.84m },
            new() { Category = "skincare", HookText = "Da mụn cám, mụn đầu đen biến mất nhờ bảo bối này.", Language = "vi", PerformanceScore = 0.92m },
            new() { Category = "skincare", HookText = "So sánh hai dòng kem dưỡng ẩm phổ biến nhất.", Language = "vi", PerformanceScore = 0.85m },
            new() { Category = "skincare", HookText = "Cách chăm sóc da dầu mụn chuẩn khoa học.", Language = "vi", PerformanceScore = 0.87m },
            new() { Category = "skincare", HookText = "Serum trị thâm mụn hiệu quả bất ngờ sau 2 tuần.", Language = "vi", PerformanceScore = 0.91m },

            // --- FOOD ---
            new() { Category = "food", HookText = "Món ăn siêu cuốn này làm cực dễ tại nhà!", Language = "vi", PerformanceScore = 0.92m },
            new() { Category = "food", HookText = "Hôm nay ăn gì? Thử ngay công thức mì trộn thần thánh.", Language = "vi", PerformanceScore = 0.88m },
            new() { Category = "food", HookText = "Review quán ăn hot nhất khu phố cổ.", Language = "vi", PerformanceScore = 0.89m },
            new() { Category = "food", HookText = "Ăn thử món bánh đang làm mưa làm gió trên mạng.", Language = "vi", PerformanceScore = 0.91m },
            new() { Category = "food", HookText = "Cách làm trà sữa trân châu chuẩn vị tại nhà.", Language = "vi", PerformanceScore = 0.84m },
            new() { Category = "food", HookText = "Món ăn vặt không thể bỏ qua cho các tín đồ ẩm thực.", Language = "vi", PerformanceScore = 0.86m },
            new() { Category = "food", HookText = "Hướng dẫn nấu lẩu thái chua cay chuẩn vị.", Language = "vi", PerformanceScore = 0.85m },
            new() { Category = "food", HookText = "Bật mí công thức nước sốt chấm vạn năng.", Language = "vi", PerformanceScore = 0.87m },

            // --- GADGET ---
            new() { Category = "gadget", HookText = "Trên tay chiếc tai nghe bluetooth chống ồn giá học sinh.", Language = "vi", PerformanceScore = 0.94m },
            new() { Category = "gadget", HookText = "Đánh giá chi tiết chiếc bàn phím cơ gõ siêu sướng tai.", Language = "vi", PerformanceScore = 0.91m },
            new() { Category = "gadget", HookText = "Mở hộp chiếc loa bluetooth mini âm thanh cực chất.", Language = "vi", PerformanceScore = 0.88m },
            new() { Category = "gadget", HookText = "Đây là chiếc sạc dự phòng sạc nhanh tiện lợi nhất.", Language = "vi", PerformanceScore = 0.87m },
            new() { Category = "gadget", HookText = "Lý do bạn nên sở hữu chiếc giá đỡ điện thoại thông minh này.", Language = "vi", PerformanceScore = 0.82m },
            new() { Category = "gadget", HookText = "Review chân thực chiếc đồng hồ thông minh nhiều tính năng.", Language = "vi", PerformanceScore = 0.85m },
            new() { Category = "gadget", HookText = "Sản phẩm công nghệ đáng mua nhất năm nay.", Language = "vi", PerformanceScore = 0.89m },
            new() { Category = "gadget", HookText = "Trải nghiệm chiếc chuột không dây silent click siêu êm.", Language = "vi", PerformanceScore = 0.86m },

            // --- HOME ---
            new() { Category = "home", HookText = "Decor lại góc làm việc cực chill với chi phí cực thấp.", Language = "vi", PerformanceScore = 0.93m },
            new() { Category = "home", HookText = "Đồ dùng tiện ích nhà bếp giúp cuộc sống dễ dàng hơn.", Language = "vi", PerformanceScore = 0.90m },
            new() { Category = "home", HookText = "Đập hộp chiếc máy hút bụi cầm tay mini siêu mạnh.", Language = "vi", PerformanceScore = 0.88m },
            new() { Category = "home", HookText = "Review chiếc đèn ngủ led hoàng hôn đang hot trend.", Language = "vi", PerformanceScore = 0.86m },
            new() { Category = "home", HookText = "Hộp đựng giày thông minh giúp tiết kiệm diện tích.", Language = "vi", PerformanceScore = 0.81m },
            new() { Category = "home", HookText = "Chiếc thảm chùi chân siêu thấm hút nước.", Language = "vi", PerformanceScore = 0.80m }
        };

        await db.HookTemplates.AddRangeAsync(hooks);
        await db.SaveChangesAsync();
    }

    public static async Task SeedModelImagesAsync(AppDbContext db)
    {
        if (await db.ModelImages.AnyAsync())
            return;

        var models = new List<ModelImage>
        {
            // 5 Females
            new() { Name = "Nữ - Casual - Thuỳ Trang", ObjectKey = "models/female_casual.jpg", Gender = "female", Style = "casual", Ethnicity = "vietnamese" },
            new() { Name = "Nữ - Trendy - Ly Ly", ObjectKey = "models/female_trendy.jpg", Gender = "female", Style = "trendy", Ethnicity = "vietnamese" },
            new() { Name = "Nữ - Formal - Minh Thư", ObjectKey = "models/female_formal.jpg", Gender = "female", Style = "formal", Ethnicity = "vietnamese" },
            new() { Name = "Nữ - Sporty - Lan Anh", ObjectKey = "models/female_sporty.jpg", Gender = "female", Style = "sporty", Ethnicity = "vietnamese" },
            new() { Name = "Nữ - Minimal - Hà Giang", ObjectKey = "models/female_minimal.jpg", Gender = "female", Style = "minimal", Ethnicity = "vietnamese" },

            // 3 Males
            new() { Name = "Nam - Casual - Minh Quân", ObjectKey = "models/male_casual.jpg", Gender = "male", Style = "casual", Ethnicity = "vietnamese" },
            new() { Name = "Nam - Smart Casual - Hoàng Long", ObjectKey = "models/male_smart_casual.jpg", Gender = "male", Style = "smart casual", Ethnicity = "vietnamese" },
            new() { Name = "Nam - Sporty - Quốc Bảo", ObjectKey = "models/male_sporty.jpg", Gender = "male", Style = "sporty", Ethnicity = "vietnamese" },

            // 2 Unisex Poses
            new() { Name = "Unisex - Casual - Hải Yến", ObjectKey = "models/unisex_pose1.jpg", Gender = "unisex", Style = "casual", Ethnicity = "vietnamese" },
            new() { Name = "Unisex - Sporty - Tuấn Kiệt", ObjectKey = "models/unisex_pose2.jpg", Gender = "unisex", Style = "sporty", Ethnicity = "vietnamese" }
        };

        await db.ModelImages.AddRangeAsync(models);
        await db.SaveChangesAsync();
    }
}
