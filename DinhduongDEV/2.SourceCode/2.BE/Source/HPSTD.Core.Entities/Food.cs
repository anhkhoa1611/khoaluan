using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPSTD.Core.Entities
{
    public class Food : BaseEntity
    {
         public string ma_thuc_pham { get; set; }
          public string ten_thuc_pham { get; set; }
          public string ma_nhom_thuc_pham { get; set; }
          public string url_anh { get; set; }
          public double nuoc { get; set; }
          public double nang_luong { get; set; }
          public double protein { get; set; }
          public double lipid { get; set; }
          public double glucid { get; set; }
          public double celluloza { get; set; }
          public double tro { get; set; }
          public double duong_tong_so { get; set; }
          public double galactoza { get; set; }
          public double maltoza { get; set; }
          public double lactoza { get; set; }
          public double fructoza { get; set; }
          public double glucoza { get; set; }
          public double sacaroza { get; set; }
          public double calci { get; set; }
          public double sat { get; set; }
          public double magie { get; set; }
          public double mangan { get; set; }
          public double phospho { get; set; }
          public double kali { get; set; }
          public double natri { get; set; }
          public double kem { get; set; }
          public double dong { get; set; }
          public double selen { get; set; }
          public double vitaminc { get; set; }
          public double vitaminb1 { get; set; }
          public double vitaminb2 { get; set; }
          public double vitaminpp { get; set; }
          public double vitaminb5 { get; set; }
          public double vitaminb6 { get; set; }
          public double folat { get; set; }
          public double vitaminb9 { get; set; }
          public double vitaminh { get; set; }
          public double vitaminb12 { get; set; }
          public double vitamina { get; set; }
          public double vitamind { get; set; }
          public double vitamine { get; set; }
          public double vitamink { get; set; }
          public double beta_caroten { get; set; }
          public double alpha_caroten { get; set; }
          public double beta_cryptoxanthin { get; set; }
          public double lycopen { get; set; }
          public double lutein_zeaxanthin { get; set; }
          public double purin { get; set; }
          public double tong_so_isoflavon { get; set; }
          public double daidzein { get; set; }
          public double genistein { get; set; }
          public double glycetin { get; set; }
          public double tong_so_acid_beo_no { get; set; }
          public double palmitic_c16 { get; set; }
          public double margaric_c17 { get; set; }
          public double stearic_c18 { get; set; }
          public double arachidic_c20 { get; set; }
          public double behenic_c22 { get; set; }
          public double lignoceric_c24 { get; set; }
          public double tong_so_acid_beo_khong_no_mot_noi_doi { get; set; }
          public double myrictoleic_c14 { get; set; }
          public double palmitoleic_c16 { get; set; }
          public double oleic_c18 { get; set; }
          public double tong_so_acid_beo_khong_no_nhieu_noi_doi { get; set; }
          public double linoleic_c18 { get; set; }
          public double linoleic_c18_n3 { get; set; }
          public double arachidonic_c20 { get; set; }
          public double eicosapentaenoic_c20 { get; set; }
          public double docosahexaenoic_c22 { get; set; }
          public double tong_so_acid_beo_trans { get; set; }
          public double cholesterol { get; set; }
          public double phytosterol { get; set; }
          public double lysin { get; set; }
          public double methionin { get; set; }
          public double tryptophan { get; set; }
          public double phenylalanin { get; set; }
          public double threonin { get; set; }
          public double valin { get; set; }
          public double leucin { get; set; }
          public double isoleucin { get; set; }
          public double arginin { get; set; }
          public double histidin { get; set; }
          public double cystin { get; set; }
          public double tyrosin { get; set; }
          public double alanin { get; set; }
          public double acid_aspartic { get; set; }
          public double acid_glutamic { get; set; }
          public double glycin { get; set; }
          public double prolin { get; set; }
          public double serin { get; set; }

        [Ignore]
        public string ten_nhom_thuc_pham { get; set; }
       
    }
}
