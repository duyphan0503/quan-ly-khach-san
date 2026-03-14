namespace HotelManagement.Core.Models;

public class WebsiteSettings
{
    public string LogoUrl { get; set; } = "/images/logo.svg";

    public string BrandMain { get; set; } = "MINH QUANG";
    public string BrandHighlight { get; set; } = "HOTEL";
    public string BrandSlogan { get; set; } = "Luxury Hotel & Resort";
    public string BrandFullName { get; set; } = "Minh Quang Hotel";

    public string ContactPhone { get; set; } = "+84 123 456 789";
    public string ContactEmail { get; set; } = "reservation@minhquang.vn";
    public string ContactAddress { get; set; } = "123 Đại lộ Hòa Bình, Quận Ninh Kiều, Thành phố Cần Thơ";
    public string FooterMapEmbedUrl { get; set; } = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3928.8475510619946!2d105.7797746757655!3d10.029444372545898!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31a0882139720a77%3A0x6717aa348f057f0!2zMTIzIMSQxrDhu51uZyBIw7JhIELDrG5oLCBOaW5oIEtp4buBdSwgQ-G6p24gVGjGoSwgVmnhu4d0IE5hbQ!5e0!3m2!1svi!2s!4v1709708000000!5m2!1svi!2s";

    public string FooterDescription { get; set; } = "Trải nghiệm sự sang trọng đỉnh cao giữa lòng thành phố. Nơi không gian ngưng đọng, và mọi cảm xúc được thăng hoa bên những dịch vụ đẳng cấp 5 sao quốc tế.";
}
