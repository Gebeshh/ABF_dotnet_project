using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestNo_9999999.Models
{
    public enum AtikCinsi
    {
        Kati,
        Toz,
        Sivi,
        AkiskanMacun
    }

    public class AtikBildirimFormu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kayıt No")]
        public string KayitNo { get; set; }

        [Required]
        [Display(Name = "Gönderen Kısım")]
        public string GonderenKisim { get; set; }

        [Required]
        [Display(Name = "Gönderen Kişi")]
        public string GonderenKisi { get; set; }

        [Required]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Atığın Cinsi alanı zorunludur.")]
        [Display(Name = "Atığın Cinsi")]
        public AtikCinsi AtikCinsi { get; set; }

        [Required(ErrorMessage = "Atığın İsmi alanı zorunludur.")]
        [Display(Name = "Atığın İsmi")]
        [StringLength(500)]
        public string AtikIsmi { get; set; }

        [Display(Name = "Sapma-DK-HTF")]
        [StringLength(500)]
        public string? SapmaDkHtf { get; set; }

        [Required(ErrorMessage = "Kg bilgisi zorunludur.")]
        [Display(Name = "Miktar (Kg)")]
        [Column(TypeName = "decimal(18, 4)")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
        public decimal MiktarKg { get; set; }

        [Display(Name = "Miktar (Adet)")]
        [Range(0, int.MaxValue, ErrorMessage = "Miktar negatif olamaz.")]
        public int? MiktarAdet { get; set; }

        [Display(Name = "Varil Adedi")]
        [Range(0, int.MaxValue, ErrorMessage = "Adet negatif olamaz.")]
        public int? AmbalajVarilAdedi { get; set; }

        [Display(Name = "Fıçı Adedi")]
        [Range(0, int.MaxValue, ErrorMessage = "Adet negatif olamaz.")]
        public int? AmbalajFiciAdedi { get; set; }

        [Display(Name = "IBC Adedi")]
        [Range(0, int.MaxValue, ErrorMessage = "Adet negatif olamaz.")]
        public int? AmbalajIbcAdedi { get; set; }

        [Display(Name = "Torba Adedi")]
        [Range(0, int.MaxValue, ErrorMessage = "Adet negatif olamaz.")]
        public int? AmbalajTorbaAdedi { get; set; }

        [Display(Name = "Kutu Adedi")]
        [Range(0, int.MaxValue, ErrorMessage = "Adet negatif olamaz.")]
        public int? AmbalajKutuAdedi { get; set; }

        [Display(Name = "Palet Adedi")]
        [Range(0, int.MaxValue, ErrorMessage = "Adet negatif olamaz.")]
        public int? AmbalajPaletAdedi { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; }

        [Display(Name = "Kısım Atık Sorumlusu")]
        public string? KisimAtikSorumlusuId { get; set; }

        [Display(Name = "ÜSM Personeli")]
        public string? UsmPersoneli { get; set; }
        
        public string? UsmPersonelId { get; set; }

        [Display(Name = "Onay Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? OnayTarihi { get; set; }
    }
}