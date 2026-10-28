using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Domain.Entity_Najm;

public partial class NajmDBContext : DbContext
{
    public NajmDBContext()
    {
    }

    public NajmDBContext(DbContextOptions<NajmDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccBase> AccBases { get; set; }

    public virtual DbSet<AccBaseUser> AccBaseUsers { get; set; }

    public virtual DbSet<AccBaseUserList> AccBaseUserLists { get; set; }

    public virtual DbSet<AccPardakht> AccPardakhts { get; set; }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Arzyabi> Arzyabis { get; set; }

    public virtual DbSet<Arzyabi1> Arzyabi1s { get; set; }

    public virtual DbSet<ArzyabiUser> ArzyabiUsers { get; set; }

    public virtual DbSet<ArzyabiUser1> ArzyabiUser1s { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<Assetsclass> Assetsclasses { get; set; }

    public virtual DbSet<Automation> Automations { get; set; }

    public virtual DbSet<CongressMain> CongressMains { get; set; }

    public virtual DbSet<CongressMainUser> CongressMainUsers { get; set; }

    public virtual DbSet<CongressMainUserArticle> CongressMainUserArticles { get; set; }

    public virtual DbSet<CongressMainUserArticlesRating> CongressMainUserArticlesRatings { get; set; }

    public virtual DbSet<CostsCode> CostsCodes { get; set; }

    public virtual DbSet<CourseAndPlace> CourseAndPlaces { get; set; }

    public virtual DbSet<Dost> Dosts { get; set; }

    public virtual DbSet<EshterakFaslnameh> EshterakFaslnamehs { get; set; }

    public virtual DbSet<FundUsersBase> FundUsersBases { get; set; }

    public virtual DbSet<FundUsersPay> FundUsersPays { get; set; }

    public virtual DbSet<FundUsersPaymentBase> FundUsersPaymentBases { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupsJaleseh> GroupsJalesehs { get; set; }

    public virtual DbSet<GroupsJalesehHzor> GroupsJalesehHzors { get; set; }

    public virtual DbSet<HamayeshParent> HamayeshParents { get; set; }

    public virtual DbSet<HamayeshPoll> HamayeshPolls { get; set; }

    public virtual DbSet<HamyeshMain> HamyeshMains { get; set; }

    public virtual DbSet<HamyeshMainPoll> HamyeshMainPolls { get; set; }

    public virtual DbSet<Implementation> Implementations { get; set; }

    public virtual DbSet<ImplementationUser> ImplementationUsers { get; set; }

    public virtual DbSet<Implementationsubject> Implementationsubjects { get; set; }

    public virtual DbSet<Javayez> Javayezs { get; set; }

    public virtual DbSet<KhadamtePajuheshi> KhadamtePajuheshis { get; set; }

    public virtual DbSet<KhadamtePajuheshi2> KhadamtePajuheshi2s { get; set; }

    public virtual DbSet<Leave> Leaves { get; set; }

    public virtual DbSet<LoginUser> LoginUsers { get; set; }

    public virtual DbSet<MediaProduct> MediaProducts { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Mission> Missions { get; set; }

    public virtual DbSet<MissionUser> MissionUsers { get; set; }

    public virtual DbSet<Poll> Polls { get; set; }

    public virtual DbSet<PublicationCdStor> PublicationCdStors { get; set; }

    public virtual DbSet<PublicationCdregister> PublicationCdregisters { get; set; }

    public virtual DbSet<PublicationCost> PublicationCosts { get; set; }

    public virtual DbSet<PublicationFactor> PublicationFactors { get; set; }

    public virtual DbSet<PublicationFactorItem> PublicationFactorItems { get; set; }

    public virtual DbSet<PublicationIndexing> PublicationIndexings { get; set; }

    public virtual DbSet<PublicationStore> PublicationStores { get; set; }

    public virtual DbSet<PublicationVerification> PublicationVerifications { get; set; }

    public virtual DbSet<PublicationsBookEdit> PublicationsBookEdits { get; set; }

    public virtual DbSet<PublicationsDesign> PublicationsDesigns { get; set; }

    public virtual DbSet<PublicationsIndexing> PublicationsIndexings { get; set; }

    public virtual DbSet<PublicationsPaging> PublicationsPagings { get; set; }

    public virtual DbSet<PublicationsPermission> PublicationsPermissions { get; set; }

    public virtual DbSet<PublicationsPrinting> PublicationsPrintings { get; set; }

    public virtual DbSet<PublicationsRegisterAssessor> PublicationsRegisterAssessors { get; set; }

    public virtual DbSet<PublicationsRegisterBook> PublicationsRegisterBooks { get; set; }

    public virtual DbSet<PublicationsRepository> PublicationsRepositories { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalaryPardakht> SalaryPardakhts { get; set; }

    public virtual DbSet<SalaryUserBase> SalaryUserBases { get; set; }

    public virtual DbSet<SalaryUserList> SalaryUserLists { get; set; }

    public virtual DbSet<SalaryUserPay> SalaryUserPays { get; set; }

    public virtual DbSet<SalaryUserPayBase> SalaryUserPayBases { get; set; }

    public virtual DbSet<SalaryUserPayGroup> SalaryUserPayGroups { get; set; }

    public virtual DbSet<SendSm> SendSms { get; set; }

    public virtual DbSet<TAcc> TAccs { get; set; }

    public virtual DbSet<TAccBase> TAccBases { get; set; }

    public virtual DbSet<TAccFactor> TAccFactors { get; set; }

    public virtual DbSet<TAccUserBase> TAccUserBases { get; set; }

    public virtual DbSet<TAccounting> TAccountings { get; set; }

    public virtual DbSet<TAmoozeshDoreh> TAmoozeshDorehs { get; set; }

    public virtual DbSet<TAmoozeshDorehStage> TAmoozeshDorehStages { get; set; }

    public virtual DbSet<TAmoozeshUserDoreh> TAmoozeshUserDorehs { get; set; }

    public virtual DbSet<TAmoozeshUserHozor> TAmoozeshUserHozors { get; set; }

    public virtual DbSet<TBanksForm> TBanksForms { get; set; }

    public virtual DbSet<TEshterakUser> TEshterakUsers { get; set; }

    public virtual DbSet<TFile> TFiles { get; set; }

    public virtual DbSet<TFiles1> TFiles1s { get; set; }

    public virtual DbSet<TKari> TKaris { get; set; }

    public virtual DbSet<TMoshavereTime> TMoshavereTimes { get; set; }

    public virtual DbSet<TMoshavereh> TMoshaverehs { get; set; }

    public virtual DbSet<TMoshaverehSabt> TMoshaverehSabts { get; set; }

    public virtual DbSet<TPayan> TPayans { get; set; }

    public virtual DbSet<TPayanCombo> TPayanCombos { get; set; }

    public virtual DbSet<TPayanCounter> TPayanCounters { get; set; }

    public virtual DbSet<TSalary> TSalaries { get; set; }

    public virtual DbSet<TTalif> TTalifs { get; set; }

    public virtual DbSet<TUser> TUsers { get; set; }

    public virtual DbSet<TUserAmoozesh> TUserAmoozeshes { get; set; }

    public virtual DbSet<TUserMoshavereh> TUserMoshaverehs { get; set; }

    public virtual DbSet<TUserSemat> TUserSemats { get; set; }

    public virtual DbSet<Taddress> Taddresses { get; set; }

    public virtual DbSet<TeacherCourse> TeacherCourses { get; set; }

    public virtual DbSet<TeacherInfo> TeacherInfos { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<UnitsUser> UnitsUsers { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Userregion> Userregions { get; set; }

    public virtual DbSet<Users2> Users2s { get; set; }

    public virtual DbSet<UsersMoshavereh> UsersMoshaverehs { get; set; }

    public virtual DbSet<UsersMoshaverehRequest> UsersMoshaverehRequests { get; set; }

    public virtual DbSet<UsersMoshaverehRequestMeet> UsersMoshaverehRequestMeets { get; set; }

    public virtual DbSet<UsersMoshaverehRequestMeetPoll> UsersMoshaverehRequestMeetPolls { get; set; }

    public virtual DbSet<Zaban> Zabans { get; set; }

    public class NajmContextFactory : IDesignTimeDbContextFactory<NajmDBContext>
    {
        public NajmDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NajmDBContext>();

            optionsBuilder.UseSqlServer(@"Data Source=192.168.0.4;Initial Catalog=DBHEKMAT;User ID=sa;Password=!@#$FGHJpoqs;TrustServerCertificate=True;Encrypt=False;", builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                builder.CommandTimeout(300);
            });

            return new NajmDBContext(optionsBuilder.Options);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(@"Data Source=192.168.0.4;Initial Catalog=DBHEKMAT;User ID=sa;Password=!@#$FGHJpoqs;TrustServerCertificate=True;Encrypt=False;", builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                builder.CommandTimeout(300);
            });
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_CI_AS");

        modelBuilder.Entity<AccBase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_acc_Base");

            entity.ToTable("acc_base");

            entity.Property(e => e.BimehHesab).HasColumnName("bimeh_hesab");
            entity.Property(e => e.BimehTakmiliHesab).HasColumnName("bimeh_takmili_hesab");
            entity.Property(e => e.Boan).HasColumnName("boan");
            entity.Property(e => e.EazafehPersent).HasColumnName("eazafeh_persent");
            entity.Property(e => e.HaghOlad).HasColumnName("hagh_olad");
            entity.Property(e => e.KaranehPercent).HasColumnName("karaneh_percent");
            entity.Property(e => e.Lock).HasColumnName("lock");
            entity.Property(e => e.MaliatPer).HasColumnName("maliat_per");
            entity.Property(e => e.Maskan).HasColumnName("maskan");
            entity.Property(e => e.MinMaliat).HasColumnName("min_maliat");
            entity.Property(e => e.PayehAsTime).HasColumnName("payeh_as_time");
            entity.Property(e => e.RegionId).HasColumnName("regionId");
            entity.Property(e => e.SandooghHesab).HasColumnName("sandoogh_hesab");
            entity.Property(e => e.SandooghName).HasColumnName("sandoogh_name");
            entity.Property(e => e.SandooghNikname).HasColumnName("sandoogh_nikname");
            entity.Property(e => e.SazmanCode).HasColumnName("sazman_code");
            entity.Property(e => e.SazmanName).HasColumnName("sazman_name");
            entity.Property(e => e.ShobehBankName).HasColumnName("shobeh_bank_name");
            entity.Property(e => e.ShobehCode).HasColumnName("shobeh_code");
            entity.Property(e => e.ShobehName).HasColumnName("shobeh_name");
            entity.Property(e => e.Taahol).HasColumnName("taahol");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Region).WithMany(p => p.AccBases)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("FK_acc_Base_region");
        });

        modelBuilder.Entity<AccBaseUser>(entity =>
        {
            entity.ToTable("acc_base_user");

            entity.Property(e => e.BankName).HasColumnName("bank_name");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Boan).HasColumnName("boan");
            entity.Property(e => e.IncomYear).HasColumnName("incom_year");
            entity.Property(e => e.JamehKol).HasColumnName("jameh_kol");
            entity.Property(e => e.Locked)
                .HasDefaultValue(false)
                .HasColumnName("locked");
            entity.Property(e => e.Maskan).HasColumnName("maskan");
            entity.Property(e => e.Masouliat)
                .HasDefaultValue(0.0, "DF_acc_base_user_masouliat")
                .HasColumnName("masouliat");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.OladCount).HasColumnName("olad_count");
            entity.Property(e => e.OladHagh).HasColumnName("olad_hagh");
            entity.Property(e => e.Payeh).HasColumnName("payeh");
            entity.Property(e => e.RegionId).HasColumnName("regionId");
            entity.Property(e => e.ShomarehHesab).HasColumnName("shomareh_hesab");
            entity.Property(e => e.Taahol)
                .HasDefaultValue(false)
                .HasColumnName("taahol");
            entity.Property(e => e.TaaholHagh).HasColumnName("taahol_hagh");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.YearCount).HasColumnName("year_count");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.AccBaseUsers)
                .HasForeignKey(d => d.Bid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_acc_base_user_acc_base");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.AccBaseUsers)
                .HasForeignKey(d => d.Uid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_acc_base_user_users");
        });

        modelBuilder.Entity<AccBaseUserList>(entity =>
        {
            entity.ToTable("acc_base_user_list");

            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Datefrom).HasColumnName("datefrom");
            entity.Property(e => e.Dateto).HasColumnName("dateto");
            entity.Property(e => e.Decription).HasColumnName("decription");
            entity.Property(e => e.FundLoanPay).HasColumnName("fund_loan_pay");
            entity.Property(e => e.FundMonthly).HasColumnName("fund_Monthly");
            entity.Property(e => e.Karaneh).HasColumnName("karaneh");
            entity.Property(e => e.Locked).HasColumnName("locked");
            entity.Property(e => e.MablaghBedehkari).HasColumnName("mablagh_bedehkari");
            entity.Property(e => e.MablaghBestankari).HasColumnName("mablagh_bestankari");
            entity.Property(e => e.MablaghBimeh).HasColumnName("mablagh_bimeh");
            entity.Property(e => e.MablaghBimehKarfarma).HasColumnName("mablagh_bimeh_karfarma");
            entity.Property(e => e.MablaghBimehKasr).HasColumnName("mablagh_bimeh_kasr");
            entity.Property(e => e.MablaghBimehTakmili).HasColumnName("mablagh_bimeh_takmili");
            entity.Property(e => e.MablaghBimehTakmiliKarfarma).HasColumnName("mablagh_bimeh_takmili_karfarma");
            entity.Property(e => e.MablaghBoan).HasColumnName("mablagh_boan");
            entity.Property(e => e.MablaghEzafehkar).HasColumnName("mablagh_ezafehkar");
            entity.Property(e => e.MablaghHghogh).HasColumnName("mablagh_hghogh");
            entity.Property(e => e.MablaghKarkard).HasColumnName("mablagh_karkard");
            entity.Property(e => e.MablaghKhalesPardakhti).HasColumnName("mablagh_khales_pardakhti");
            entity.Property(e => e.MablaghKsorat).HasColumnName("mablagh_ksorat");
            entity.Property(e => e.MablaghMaliat).HasColumnName("mablagh_maliat");
            entity.Property(e => e.MablaghMamoriat).HasColumnName("mablagh_mamoriat");
            entity.Property(e => e.MablaghMaskan).HasColumnName("mablagh_maskan");
            entity.Property(e => e.MablaghMasouliat).HasColumnName("mablagh_masouliat");
            entity.Property(e => e.MablaghModirPercent).HasColumnName("mablagh_modir_percent");
            entity.Property(e => e.MablaghOlad).HasColumnName("mablagh_olad");
            entity.Property(e => e.MablaghOther).HasColumnName("mablagh_other");
            entity.Property(e => e.MablaghSandogh).HasColumnName("mablagh_sandogh");
            entity.Property(e => e.MablaghTaahol).HasColumnName("mablagh_taahol");
            entity.Property(e => e.MablaghTashvighi).HasColumnName("mablagh_tashvighi");
            entity.Property(e => e.ModirPercent).HasColumnName("modir_percent");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Pdate).HasColumnName("pdate");
            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.SaatEzafekari).HasColumnName("saat_ezafekari");
            entity.Property(e => e.SaatKarkard).HasColumnName("saat_karkard");
            entity.Property(e => e.SaatMamoriat).HasColumnName("saat_mamoriat");
            entity.Property(e => e.SaatTashvighi).HasColumnName("saat_tashvighi");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.AccBaseUserLists)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("FK_acc_base_user_list_acc_base_user");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.AccBaseUserLists)
                .HasForeignKey(d => d.Pid)
                .HasConstraintName("FK_acc_base_user_list_acc_pardakht");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.AccBaseUserLists)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK_acc_base_user_list_T_users");
        });

        modelBuilder.Entity<AccPardakht>(entity =>
        {
            entity.ToTable("acc_pardakht");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Accb).HasColumnName("accb");
            entity.Property(e => e.Bimeh).HasColumnName("bimeh");
            entity.Property(e => e.BimehC).HasColumnName("bimeh_c");
            entity.Property(e => e.BimehCHekmat).HasColumnName("bimeh_c_hekmat");
            entity.Property(e => e.BimehHekmat)
                .HasDefaultValue(0.0, "DF_acc_pardakht_bimeh_hekmat")
                .HasColumnName("bimeh_hekmat");
            entity.Property(e => e.CodeMarhaleh).HasColumnName("Code_marhaleh");
            entity.Property(e => e.DescAll).HasColumnName("descAll");
            entity.Property(e => e.FileMatn).HasColumnName("fileMatn");
            entity.Property(e => e.FundAccount).HasColumnName("Fund_Account");
            entity.Property(e => e.FundAlias).HasColumnName("Fund_Alias");
            entity.Property(e => e.FundBank).HasColumnName("Fund_bank");
            entity.Property(e => e.FundMembersNumber).HasColumnName("fund_members_number");
            entity.Property(e => e.FundName).HasColumnName("Fund_Name");
            entity.Property(e => e.FundTotal).HasColumnName("fund_total");
            entity.Property(e => e.Islock).HasColumnName("islock");
            entity.Property(e => e.Khales).HasColumnName("khales");
            entity.Property(e => e.MablaghHrof).HasColumnName("Mablagh_hrof");
            entity.Property(e => e.Maliat).HasColumnName("maliat");
            entity.Property(e => e.Tedad).HasColumnName("tedad");
            entity.Property(e => e.Vdate).HasColumnName("VDate");

            entity.HasOne(d => d.AccbNavigation).WithMany(p => p.AccPardakhts)
                .HasForeignKey(d => d.Accb)
                .HasConstraintName("FK_acc_pardakht_acc_base");
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLog");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Ip).HasColumnName("IP");
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
        });

        modelBuilder.Entity<Arzyabi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Arzyabi_1");

            entity.ToTable("Arzyabi");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Chap).HasColumnName("chap");
            entity.Property(e => e.CodeMaghaleh).HasColumnName("code_maghaleh");
            entity.Property(e => e.CodeNevisandeh).HasColumnName("Code_nevisandeh");
            entity.Property(e => e.Dalil).HasColumnName("dalil");
            entity.Property(e => e.Datedaryaft).HasColumnName("datedaryaft");
            entity.Property(e => e.Dateersal).HasColumnName("dateersal");
            entity.Property(e => e.Emtiaz).HasColumnName("emtiaz");
            entity.Property(e => e.EslahatMohtavaee).HasColumnName("Eslahat_Mohtavaee");
            entity.Property(e => e.EslahatSori).HasColumnName("eslahat_sori");
            entity.Property(e => e.Maghalehname).HasColumnName("maghalehname");
            entity.Property(e => e.Melak).HasColumnName("melak");
            entity.Property(e => e.Mortabet).HasColumnName("mortabet");
            entity.Property(e => e.Natijeh).HasColumnName("natijeh");
            entity.Property(e => e.Nazarat).HasColumnName("nazarat");
            entity.Property(e => e.Nevisandeh).HasColumnName("nevisandeh");
            entity.Property(e => e.TozihVanazar).HasColumnName("tozih_vanazar");
            entity.Property(e => e.Tozihat).HasColumnName("tozihat");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Arzyabi1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Arzyabi");

            entity.ToTable("Arzyabi_1");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Chap).HasColumnName("chap");
            entity.Property(e => e.CodeMaghaleh).HasColumnName("code_maghaleh");
            entity.Property(e => e.CodeNevisandeh).HasColumnName("Code_nevisandeh");
            entity.Property(e => e.Dalil).HasColumnName("dalil");
            entity.Property(e => e.Datedaryaft).HasColumnName("datedaryaft");
            entity.Property(e => e.Dateersal).HasColumnName("dateersal");
            entity.Property(e => e.Emtiaz).HasColumnName("emtiaz");
            entity.Property(e => e.EslahatMohtavaee).HasColumnName("Eslahat_Mohtavaee");
            entity.Property(e => e.EslahatSori).HasColumnName("eslahat_sori");
            entity.Property(e => e.Maghalehname).HasColumnName("maghalehname");
            entity.Property(e => e.Melak).HasColumnName("melak");
            entity.Property(e => e.Mortabet).HasColumnName("mortabet");
            entity.Property(e => e.Natijeh).HasColumnName("natijeh");
            entity.Property(e => e.Nazarat).HasColumnName("nazarat");
            entity.Property(e => e.Nevisandeh).HasColumnName("nevisandeh");
            entity.Property(e => e.TozihVanazar).HasColumnName("tozih_vanazar");
            entity.Property(e => e.Tozihat).HasColumnName("tozihat");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<ArzyabiUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Arzyabi_user_1");

            entity.ToTable("Arzyabi_user");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Codemelli).HasColumnName("codemelli");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.F2).HasColumnName("f2");
            entity.Property(e => e.F3).HasColumnName("f3");
            entity.Property(e => e.Tedadmaghalat).HasColumnName("tedadmaghalat");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<ArzyabiUser1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Arzyabi_user");

            entity.ToTable("Arzyabi_user_1");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Codemelli).HasColumnName("codemelli");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.F2).HasColumnName("f2");
            entity.Property(e => e.F3).HasColumnName("f3");
            entity.Property(e => e.Tedadmaghalat).HasColumnName("tedadmaghalat");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("assets");

            entity.Property(e => e.ClassId).HasColumnName("classId");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Docno).HasColumnName("docno");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Regionid).HasColumnName("regionid");
            entity.Property(e => e.Secenassetno).HasColumnName("secenassetno");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Unit).HasColumnName("unit");

            entity.HasOne(d => d.Class).WithMany(p => p.Assets)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_assets_assetsclass");

            entity.HasOne(d => d.PlaceNavigation).WithMany(p => p.AssetPlaceNavigations)
                .HasForeignKey(d => d.Place)
                .HasConstraintName("FK_assets_units1");

            entity.HasOne(d => d.Region).WithMany(p => p.Assets)
                .HasForeignKey(d => d.Regionid)
                .HasConstraintName("FK_assets_Regions");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.AssetUnitNavigations)
                .HasForeignKey(d => d.Unit)
                .HasConstraintName("FK_assets_units");
        });

        modelBuilder.Entity<Assetsclass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__assetscl__3214EC07614F9259");

            entity.ToTable("assetsclass");
        });

        modelBuilder.Entity<Automation>(entity =>
        {
            entity.ToTable("automation");

            entity.Property(e => e.Attach).HasColumnName("attach");
            entity.Property(e => e.Dateseen).HasColumnName("dateseen");
            entity.Property(e => e.Datesend).HasColumnName("datesend");
            entity.Property(e => e.Draft).HasColumnName("draft");
            entity.Property(e => e.Gid).HasColumnName("gid");
            entity.Property(e => e.Islock).HasColumnName("islock");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.Reciver).HasColumnName("reciver");
            entity.Property(e => e.Rsecivertype).HasColumnName("rsecivertype");
            entity.Property(e => e.Sender).HasColumnName("sender");
            entity.Property(e => e.Subject)
                .HasMaxLength(100)
                .HasColumnName("subject");

            entity.HasOne(d => d.ReciverNavigation).WithMany(p => p.AutomationReciverNavigations)
                .HasForeignKey(d => d.Reciver)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_automation_LoginUsers");

            entity.HasOne(d => d.SenderNavigation).WithMany(p => p.AutomationSenderNavigations)
                .HasForeignKey(d => d.Sender)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_automation_automation1");

            entity.HasOne(d => d.TopMessageNavigation).WithMany(p => p.InverseTopMessageNavigation)
                .HasForeignKey(d => d.TopMessage)
                .HasConstraintName("FK_automation_automation");
        });

        modelBuilder.Entity<CongressMain>(entity =>
        {
            entity.ToTable("congress_main");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdedDate).HasColumnName("aded_date");
            entity.Property(e => e.Charachterinpage).HasColumnName("charachterinpage");
            entity.Property(e => e.DateFrom).HasColumnName("Date_from");
            entity.Property(e => e.DateTo).HasColumnName("Date_to");
            entity.Property(e => e.Makan).HasColumnName("makan");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.OnlyMjma).HasColumnName("only_mjma");
            entity.Property(e => e.OtherSponser).HasColumnName("Other_Sponser");
        });

        modelBuilder.Entity<CongressMainUser>(entity =>
        {
            entity.ToTable("congress_main_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedDate).HasColumnName("added_date");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Codemelli)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("codemelli");
            entity.Property(e => e.Congid).HasColumnName("congid");
            entity.Property(e => e.Decription).HasColumnName("decription");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Fname).HasColumnName("fname");
            entity.Property(e => e.Lname).HasColumnName("lname");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Cong).WithMany(p => p.CongressMainUsers)
                .HasForeignKey(d => d.Congid)
                .HasConstraintName("FK_congress_main_user_congress_main");
        });

        modelBuilder.Entity<CongressMainUserArticle>(entity =>
        {
            entity.ToTable("congress_main_user_Articles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.AdedDate).HasColumnName("aded_date");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Congid).HasColumnName("congid");
            entity.Property(e => e.FullReciveDate).HasColumnName("full_recive_date");
            entity.Property(e => e.Language).HasColumnName("language");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Nofword).HasColumnName("nofword");
            entity.Property(e => e.ReciveDate).HasColumnName("recive_date");
            entity.Property(e => e.Tedadsafheh).HasColumnName("tedadsafheh");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.Cong).WithMany(p => p.CongressMainUserArticles)
                .HasForeignKey(d => d.Congid)
                .HasConstraintName("FK_congress_main_user_Articles_congress_main");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.CongressMainUserArticles)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK_congress_main_user_Articles_congress_main_user");
        });

        modelBuilder.Entity<CongressMainUserArticlesRating>(entity =>
        {
            entity.ToTable("congress_main_user_Articles_Rating");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accept).HasColumnName("accept");
            entity.Property(e => e.AllowAbstract).HasColumnName("allow_abstract");
            entity.Property(e => e.AllowPrint).HasColumnName("allow_print");
            entity.Property(e => e.AllowShow).HasColumnName("allow_show");
            entity.Property(e => e.AllowSpeech).HasColumnName("allow_speech");
            entity.Property(e => e.ArticleId).HasColumnName("Article_id");
            entity.Property(e => e.Congid).HasColumnName("congid");
            entity.Property(e => e.DateRecive).HasColumnName("date_recive");
            entity.Property(e => e.DateSend).HasColumnName("date_send");
            entity.Property(e => e.Decription).HasColumnName("decription");
            entity.Property(e => e.Mortabet).HasColumnName("mortabet");
            entity.Property(e => e.Nazar).HasColumnName("nazar");
            entity.Property(e => e.Reform).HasColumnName("reform");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.UidAuthor).HasColumnName("uid_Author");
            entity.Property(e => e.UidPreferential).HasColumnName("uid_preferential");

            entity.HasOne(d => d.Article).WithMany(p => p.CongressMainUserArticlesRatings)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("FK_congress_main_user_Articles_Rating_congress_main_user_Articles");

            entity.HasOne(d => d.Cong).WithMany(p => p.CongressMainUserArticlesRatings)
                .HasForeignKey(d => d.Congid)
                .HasConstraintName("FK_congress_main_user_Articles_Rating_congress_main_user_Articles_Rating");

            entity.HasOne(d => d.UidAuthorNavigation).WithMany(p => p.CongressMainUserArticlesRatingUidAuthorNavigations)
                .HasForeignKey(d => d.UidAuthor)
                .HasConstraintName("FK_congress_main_user_Articles_Rating_congress_main_user1");

            entity.HasOne(d => d.UidPreferentialNavigation).WithMany(p => p.CongressMainUserArticlesRatingUidPreferentialNavigations)
                .HasForeignKey(d => d.UidPreferential)
                .HasConstraintName("FK_congress_main_user_Articles_Rating_congress_main_user");
        });

        modelBuilder.Entity<CostsCode>(entity =>
        {
            entity.ToTable("CostsCode");

            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<CourseAndPlace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Tinfo");

            entity.ToTable("CourseAndPlace");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseName)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("courseName");
            entity.Property(e => e.Hours)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("hours");
            entity.Property(e => e.Place)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("place");
            entity.Property(e => e.Year)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("year");
        });

        modelBuilder.Entity<Dost>(entity =>
        {
            entity.ToTable("Dost");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Daddress)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("DAddress");
            entity.Property(e => e.Dname)
                .HasMaxLength(100)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("DName");
            entity.Property(e => e.MahalKar)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Semat)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Dosts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Dost_T_Users");
        });

        modelBuilder.Entity<EshterakFaslnameh>(entity =>
        {
            entity.ToTable("Eshterak_faslnameh");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Codeposti)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Credit).HasColumnName("credit");
            entity.Property(e => e.Eshtaerakno)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("eshtaerakno");
            entity.Property(e => e.Fame)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Lname)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Tel)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("tel");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<FundUsersBase>(entity =>
        {
            entity.ToTable("Fund_Users_base");

            entity.HasIndex(e => e.Uid, "IX_Fund_Users_base").IsUnique();

            entity.Property(e => e.Fid).HasColumnName("FID");
            entity.Property(e => e.MonthlyFee).HasColumnName("Monthly_fee");
            entity.Property(e => e.Paid).HasColumnName("paid");
            entity.Property(e => e.PaymentFee).HasColumnName("payment_fee");
            entity.Property(e => e.PaymentNumber).HasColumnName("payment_number");
            entity.Property(e => e.PaymentTotal).HasColumnName("payment_total");
            entity.Property(e => e.Region).HasColumnName("region");
            entity.Property(e => e.SalaryFraction).HasColumnName("salary_fraction");
            entity.Property(e => e.StartDate).HasColumnName("Start_date");
            entity.Property(e => e.StartFee).HasColumnName("Start_fee");
            entity.Property(e => e.Uid).HasColumnName("UID");

            entity.HasOne(d => d.RegionNavigation).WithMany(p => p.FundUsersBases)
                .HasForeignKey(d => d.Region)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fund_Users_base_Regions");
        });

        modelBuilder.Entity<FundUsersPay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Fund_User_Pay");

            entity.ToTable("Fund_Users_Pay");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Fub).HasColumnName("FUB");
            entity.Property(e => e.Fupb).HasColumnName("FUPB");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Paid).HasColumnName("paid");
            entity.Property(e => e.PayDate).HasColumnName("Pay_date");
            entity.Property(e => e.PayMethod).HasColumnName("pay_method");
            entity.Property(e => e.Paytype).HasColumnName("paytype");
            entity.Property(e => e.SalaryFraction).HasColumnName("salary_fraction");
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.FubNavigation).WithMany(p => p.FundUsersPays)
                .HasForeignKey(d => d.Fub)
                .HasConstraintName("FK_Fund_Users_Pay_Fund_Users_base");

            entity.HasOne(d => d.FupbNavigation).WithMany(p => p.FundUsersPays)
                .HasForeignKey(d => d.Fupb)
                .HasConstraintName("FK_Fund_Users_Pay_Fund_Users_Payment_base");
        });

        modelBuilder.Entity<FundUsersPaymentBase>(entity =>
        {
            entity.ToTable("Fund_Users_Payment_base");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fub).HasColumnName("FUB");
            entity.Property(e => e.FundType).HasColumnName("fund_type");
            entity.Property(e => e.InstallmentFee).HasColumnName("installment_fee");
            entity.Property(e => e.InstallmentFeeFirst).HasColumnName("installment_fee_first");
            entity.Property(e => e.InstallmentFeeLast).HasColumnName("installment_fee_last");
            entity.Property(e => e.InstallmentNumber).HasColumnName("installment_number");
            entity.Property(e => e.LoanNumber).HasColumnName("Loan_number");
            entity.Property(e => e.Paid).HasColumnName("paid");
            entity.Property(e => e.PayDate).HasColumnName("pay_date");
            entity.Property(e => e.PayFirst).HasColumnName("pay_first");
            entity.Property(e => e.PayLast).HasColumnName("pay_last");
            entity.Property(e => e.RemainedNumber).HasColumnName("Remained_number");
            entity.Property(e => e.SalaryFraction).HasColumnName("salary_fraction");
            entity.Property(e => e.TotalFee).HasColumnName("Total_fee");
            entity.Property(e => e.Uid).HasColumnName("UID");

            entity.HasOne(d => d.FubNavigation).WithMany(p => p.FundUsersPaymentBases)
                .HasForeignKey(d => d.Fub)
                .HasConstraintName("FK_Fund_Users_Payment_base_Fund_Users_base");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId);

            entity.Property(e => e.GroupId)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("GroupID");
            entity.Property(e => e.AsasNameh).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.BuildDate).HasColumnType("datetime");
            entity.Property(e => e.DabirId).HasColumnName("DabirID");
            entity.Property(e => e.Description).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.GroupName).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.LastJaleseh).HasColumnType("datetime");
            entity.Property(e => e.Pajoheshid).HasColumnName("pajoheshid");
            entity.Property(e => e.RoozJalaseh)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Dabir).WithMany(p => p.GroupDabirs)
                .HasForeignKey(d => d.DabirId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Groups_T_Users1");

            entity.HasOne(d => d.Pajohesh).WithMany(p => p.GroupPajoheshes)
                .HasForeignKey(d => d.Pajoheshid)
                .HasConstraintName("FK_Groups_T_Users2");

            entity.HasOne(d => d.Region).WithMany(p => p.Groups)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("FK_Groups_Regions");

            entity.HasOne(d => d.User).WithMany(p => p.GroupUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Groups_T_Users");
        });

        modelBuilder.Entity<GroupsJaleseh>(entity =>
        {
            entity.ToTable("Groups_jaleseh");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DabirId).HasColumnName("DabirID");
            entity.Property(e => e.Dabirname).HasColumnName("dabirname");
            entity.Property(e => e.Dastor).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ETime)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("E_Time");
            entity.Property(e => e.EraeUserName).HasColumnName("EraeUser_name");
            entity.Property(e => e.GroupId)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("GroupID");
            entity.Property(e => e.Mahal).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.ManagerId).HasColumnName("ManagerID");
            entity.Property(e => e.Mozakerat).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.NDastor)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("N_Dastor");
            entity.Property(e => e.Note).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Ntype).HasColumnName("NType");
            entity.Property(e => e.Onvan).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.STime)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("S_Time");
            entity.Property(e => e.Tag)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("tag");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupsJalesehs)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Groups_jaleseh_Groups");
        });

        modelBuilder.Entity<GroupsJalesehHzor>(entity =>
        {
            entity.ToTable("Groups_jaleseh_hzor");

            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Fname).HasColumnName("fname");
            entity.Property(e => e.Jalesehid).HasColumnName("jalesehid");
            entity.Property(e => e.Lname).HasColumnName("lname");
            entity.Property(e => e.TIn).HasColumnName("t_in");
            entity.Property(e => e.TOut).HasColumnName("t_out");

            entity.HasOne(d => d.Jaleseh).WithMany(p => p.GroupsJalesehHzors)
                .HasForeignKey(d => d.Jalesehid)
                .HasConstraintName("FK_Groups_jaleseh_hzor_Groups_jaleseh");
        });

        modelBuilder.Entity<HamayeshParent>(entity =>
        {
            entity.ToTable("Hamayesh_parent");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Kar4).HasColumnName("kar4");
            entity.Property(e => e.Kar5).HasColumnName("kar5");
            entity.Property(e => e.Kar6).HasColumnName("kar6");
            entity.Property(e => e.Kar7).HasColumnName("kar7");
            entity.Property(e => e.Makan).HasColumnName("makan");
            entity.Property(e => e.Time).HasColumnName("time");
        });

        modelBuilder.Entity<HamayeshPoll>(entity =>
        {
            entity.ToTable("Hamayesh_poll");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.HamId).HasColumnName("ham_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Pishnahad).HasColumnName("pishnahad");
            entity.Property(e => e.Pol1).HasColumnName("pol1");
            entity.Property(e => e.Pol10).HasColumnName("pol10");
            entity.Property(e => e.Pol11).HasColumnName("pol11");
            entity.Property(e => e.Pol2).HasColumnName("pol2");
            entity.Property(e => e.Pol3).HasColumnName("pol3");
            entity.Property(e => e.Pol4).HasColumnName("pol4");
            entity.Property(e => e.Pol5).HasColumnName("pol5");
            entity.Property(e => e.Pol6).HasColumnName("pol6");
            entity.Property(e => e.Pol7).HasColumnName("pol7");
            entity.Property(e => e.Pol8).HasColumnName("pol8");
            entity.Property(e => e.Pol9).HasColumnName("pol9");
            entity.Property(e => e.PolKar1).HasColumnName("pol_kar1");
            entity.Property(e => e.PolKar2).HasColumnName("pol_kar2");
            entity.Property(e => e.PolKar3).HasColumnName("pol_kar3");
            entity.Property(e => e.PolKar4).HasColumnName("pol_kar4");
            entity.Property(e => e.PolKar5).HasColumnName("pol_kar5");
            entity.Property(e => e.PolKar6).HasColumnName("pol_kar6");
            entity.Property(e => e.PolKar7).HasColumnName("pol_kar7");
            entity.Property(e => e.Sen).HasColumnName("sen");
            entity.Property(e => e.Tabligh).HasColumnName("tabligh");
            entity.Property(e => e.TahsialtKelasic).HasColumnName("tahsialt_kelasic");
            entity.Property(e => e.TahsilatHowzeh).HasColumnName("tahsilat_howzeh");
        });

        modelBuilder.Entity<HamyeshMain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HamyeshMain");

            entity.ToTable("Hamyesh_Main");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Makan).HasColumnName("makan");
            entity.Property(e => e.PollText1).HasColumnName("poll_Text_1");
            entity.Property(e => e.PollText10).HasColumnName("poll_Text_10");
            entity.Property(e => e.PollText11).HasColumnName("poll_Text_11");
            entity.Property(e => e.PollText12).HasColumnName("poll_Text_12");
            entity.Property(e => e.PollText13).HasColumnName("poll_Text_13");
            entity.Property(e => e.PollText14).HasColumnName("poll_Text_14");
            entity.Property(e => e.PollText15).HasColumnName("poll_Text_15");
            entity.Property(e => e.PollText2).HasColumnName("poll_Text_2");
            entity.Property(e => e.PollText3).HasColumnName("poll_Text_3");
            entity.Property(e => e.PollText4).HasColumnName("poll_Text_4");
            entity.Property(e => e.PollText5).HasColumnName("poll_Text_5");
            entity.Property(e => e.PollText6).HasColumnName("poll_Text_6");
            entity.Property(e => e.PollText7).HasColumnName("poll_Text_7");
            entity.Property(e => e.PollText8).HasColumnName("poll_Text_8");
            entity.Property(e => e.PollText9).HasColumnName("poll_Text_9");
            entity.Property(e => e.Time).HasColumnName("time");

            entity.HasOne(d => d.Region).WithMany(p => p.HamyeshMains)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("FK_Hamyesh_Main_Hamyesh_Main");
        });

        modelBuilder.Entity<HamyeshMainPoll>(entity =>
        {
            entity.ToTable("Hamyesh_Main_poll");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.HamId).HasColumnName("ham_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Pol1).HasColumnName("pol1");
            entity.Property(e => e.Pol10).HasColumnName("pol10");
            entity.Property(e => e.Pol11).HasColumnName("pol11");
            entity.Property(e => e.Pol12).HasColumnName("pol12");
            entity.Property(e => e.Pol13).HasColumnName("pol13");
            entity.Property(e => e.Pol14).HasColumnName("pol14");
            entity.Property(e => e.Pol15).HasColumnName("pol15");
            entity.Property(e => e.Pol2).HasColumnName("pol2");
            entity.Property(e => e.Pol3).HasColumnName("pol3");
            entity.Property(e => e.Pol4).HasColumnName("pol4");
            entity.Property(e => e.Pol5).HasColumnName("pol5");
            entity.Property(e => e.Pol6).HasColumnName("pol6");
            entity.Property(e => e.Pol7).HasColumnName("pol7");
            entity.Property(e => e.Pol8).HasColumnName("pol8");
            entity.Property(e => e.Pol9).HasColumnName("pol9");
            entity.Property(e => e.Sen).HasColumnName("sen");
            entity.Property(e => e.TahsialtKelasic).HasColumnName("tahsialt_kelasic");
            entity.Property(e => e.TahsilatHowzeh).HasColumnName("tahsilat_howzeh");

            entity.HasOne(d => d.Ham).WithMany(p => p.HamyeshMainPolls)
                .HasForeignKey(d => d.HamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Hamyesh_Main_poll_Hamyesh_Main");
        });

        modelBuilder.Entity<Implementation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Implemen__3214EC07731A80F0");

            entity.ToTable("Implementation");

            entity.Property(e => e.EditDate).HasColumnName("edit_date");
            entity.Property(e => e.Executive).HasColumnName("executive");
            entity.Property(e => e.Executiveid).HasColumnName("executiveid");
            entity.Property(e => e.ImplementationInDate).HasColumnName("implementation_in_date");
            entity.Property(e => e.ImplementationOutDate).HasColumnName("implementation_out_date");
            entity.Property(e => e.Numofmin).HasColumnName("numofmin");
            entity.Property(e => e.Numofword).HasColumnName("numofword");
            entity.Property(e => e.OppositionInDate).HasColumnName("opposition_in_date");
            entity.Property(e => e.OppositionOutDate).HasColumnName("opposition_out_date");
            entity.Property(e => e.PayDate).HasColumnName("pay_date");
            entity.Property(e => e.RegisterDate).HasColumnName("register_date");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.Subjectid).HasColumnName("subjectid");
            entity.Property(e => e.TypeDate).HasColumnName("type_date");

            entity.HasOne(d => d.ExecutiveNavigation).WithMany(p => p.Implementations)
                .HasForeignKey(d => d.Executiveid)
                .HasConstraintName("FK_Implementation_ImplementationUsers");

            entity.HasOne(d => d.SubjectNavigation).WithMany(p => p.Implementations)
                .HasForeignKey(d => d.Subjectid)
                .HasConstraintName("FK_Implementation_Implementationsubjects");
        });

        modelBuilder.Entity<ImplementationUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Implemen__3214EC07FC5294B0");

            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Firstname).HasColumnName("firstname");
            entity.Property(e => e.Lastname).HasColumnName("lastname");
            entity.Property(e => e.ShHesab).HasColumnName("sh_hesab");
            entity.Property(e => e.User).HasColumnName("user");
        });

        modelBuilder.Entity<Implementationsubject>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Subject).HasColumnName("subject");
        });

        modelBuilder.Entity<Javayez>(entity =>
        {
            entity.ToTable("Javayez");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Moshakhasat).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Javayezs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Javayez_T_Users");
        });

        modelBuilder.Entity<KhadamtePajuheshi>(entity =>
        {
            entity.ToTable("khadamtePajuheshi");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateOfComp)
                .HasMaxLength(10)
                .HasColumnName("dateOfComp");
            entity.Property(e => e.FinalPageCount).HasColumnName("finalPageCount");
            entity.Property(e => e.FinalWordCount).HasColumnName("finalWordCount");
            entity.Property(e => e.HwPageCount).HasColumnName("hwPageCount");
            entity.Property(e => e.Penman)
                .HasMaxLength(200)
                .HasColumnName("penman");
            entity.Property(e => e.PresentationDate)
                .HasMaxLength(10)
                .HasColumnName("presentationDate");
            entity.Property(e => e.PrimaryWordCount).HasColumnName("primaryWordCount");
            entity.Property(e => e.Refers)
                .HasMaxLength(200)
                .HasColumnName("refers");
            entity.Property(e => e.RegisterationNum)
                .HasMaxLength(50)
                .HasColumnName("registerationNum");
            entity.Property(e => e.Subject)
                .HasMaxLength(250)
                .HasColumnName("subject");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
            entity.Property(e => e.Transferee)
                .HasMaxLength(250)
                .HasColumnName("transferee");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.Typist)
                .HasMaxLength(200)
                .HasColumnName("typist");
            entity.Property(e => e.Writer)
                .HasMaxLength(200)
                .HasColumnName("writer");
        });

        modelBuilder.Entity<KhadamtePajuheshi2>(entity =>
        {
            entity.ToTable("khadamtePajuheshi2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Erja)
                .HasMaxLength(150)
                .HasColumnName("erja");
            entity.Property(e => e.Eslahat)
                .HasMaxLength(50)
                .HasColumnName("eslahat");
            entity.Property(e => e.KalamatNahaei).HasColumnName("kalamatNahaei");
            entity.Property(e => e.Mojri)
                .HasMaxLength(150)
                .HasColumnName("mojri");
            entity.Property(e => e.SafahatNahaei).HasColumnName("safahatNahaei");
            entity.Property(e => e.TId).HasColumnName("tId");
            entity.Property(e => e.TarikheEslah)
                .HasMaxLength(10)
                .HasColumnName("tarikheEslah");

            entity.HasOne(d => d.TIdNavigation).WithMany(p => p.KhadamtePajuheshi2s)
                .HasForeignKey(d => d.TId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_khadamtePajuheshi2_khadamtePajuheshi");
        });

        modelBuilder.Entity<Leave>(entity =>
        {
            entity.ToTable("leave");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Agreement).HasColumnName("agreement");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Dateend).HasColumnName("dateend");
            entity.Property(e => e.Datestart).HasColumnName("datestart");
            entity.Property(e => e.Parentuid1).HasColumnName("parentuid1");
            entity.Property(e => e.Parentuid2).HasColumnName("parentuid2");
            entity.Property(e => e.Timeend).HasColumnName("timeend");
            entity.Property(e => e.Timestart).HasColumnName("timestart");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Unitid).HasColumnName("unitid");

            entity.HasOne(d => d.Unit).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.Unitid)
                .HasConstraintName("FK_leave_units");
        });

        modelBuilder.Entity<LoginUser>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");

            entity.HasOne(d => d.User).WithMany(p => p.LoginUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoginUsers_T_Users");
        });

        modelBuilder.Entity<MediaProduct>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Media_product");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active1).HasColumnName("active1");
            entity.Property(e => e.Active2).HasColumnName("active2");
            entity.Property(e => e.Active3).HasColumnName("active3");
            entity.Property(e => e.Active4).HasColumnName("active4");
            entity.Property(e => e.Amelejraee1).HasColumnName("amelejraee1");
            entity.Property(e => e.Amelejraee2).HasColumnName("amelejraee2");
            entity.Property(e => e.Amelejraee3)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("amelejraee3");
            entity.Property(e => e.Amelejraee4).HasColumnName("amelejraee4");
            entity.Property(e => e.Applier1).HasColumnName("applier1");
            entity.Property(e => e.Applier2).HasColumnName("applier2");
            entity.Property(e => e.Datedelivery1).HasColumnName("datedelivery1");
            entity.Property(e => e.Datedelivery2).HasColumnName("datedelivery2");
            entity.Property(e => e.Dateproliferation).HasColumnName("dateproliferation");
            entity.Property(e => e.Daterecive1).HasColumnName("daterecive1");
            entity.Property(e => e.Daterecive2).HasColumnName("daterecive2");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Farvast).HasColumnName("farvast");
            entity.Property(e => e.Guid).HasColumnName("guid");
            entity.Property(e => e.Locationfile).HasColumnName("locationfile");
            entity.Property(e => e.Mediaid).HasColumnName("mediaid");
            entity.Property(e => e.Medianame).HasColumnName("medianame");
            entity.Property(e => e.Mediatype).HasColumnName("mediatype");
            entity.Property(e => e.Numbers).HasColumnName("numbers");
            entity.Property(e => e.Statid).HasColumnName("statid");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.Subsubject).HasColumnName("subsubject");
            entity.Property(e => e.Teacher).HasColumnName("teacher");

            entity.HasOne(d => d.Media).WithMany(p => p.Media)
                .HasForeignKey(d => d.Mediaid)
                .HasConstraintName("FK_Media_Publication_CDRegister");

            entity.HasOne(d => d.TeacherNavigation).WithMany(p => p.Media)
                .HasForeignKey(d => d.Teacher)
                .HasConstraintName("FK_Media_Users_2");
        });

        modelBuilder.Entity<Mission>(entity =>
        {
            entity.ToTable("Mission");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Agreement).HasColumnName("agreement");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Dateend).HasColumnName("dateend");
            entity.Property(e => e.Datestart).HasColumnName("datestart");
            entity.Property(e => e.Disagreennote).HasColumnName("disagreennote");
            entity.Property(e => e.MissionReason).HasColumnName("missionReason");
            entity.Property(e => e.Missionplace).HasColumnName("missionplace");
            entity.Property(e => e.Timeend).HasColumnName("timeend");
            entity.Property(e => e.Timestart).HasColumnName("timestart");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Unitid).HasColumnName("unitid");
            entity.Property(e => e.Vehicle).HasColumnName("vehicle");

            entity.HasOne(d => d.Unit).WithMany(p => p.Missions)
                .HasForeignKey(d => d.Unitid)
                .HasConstraintName("FK_Mission_units");
        });

        modelBuilder.Entity<MissionUser>(entity =>
        {
            entity.ToTable("Mission_users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Missid).HasColumnName("missid");
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.Miss).WithMany(p => p.MissionUsers)
                .HasForeignKey(d => d.Missid)
                .HasConstraintName("FK_Mission_users_Mission");
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.ToTable("poll");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Ostadid).HasColumnName("ostadid");
            entity.Property(e => e.Pol1).HasColumnName("pol1");
            entity.Property(e => e.Pol10).HasColumnName("pol10");
            entity.Property(e => e.Pol11).HasColumnName("pol11");
            entity.Property(e => e.Pol12).HasColumnName("pol12");
            entity.Property(e => e.Pol13).HasColumnName("pol13");
            entity.Property(e => e.Pol14).HasColumnName("pol14");
            entity.Property(e => e.Pol15).HasColumnName("pol15");
            entity.Property(e => e.Pol16).HasColumnName("pol16");
            entity.Property(e => e.Pol2).HasColumnName("pol2");
            entity.Property(e => e.Pol3).HasColumnName("pol3");
            entity.Property(e => e.Pol4).HasColumnName("pol4");
            entity.Property(e => e.Pol5).HasColumnName("pol5");
            entity.Property(e => e.Pol6).HasColumnName("pol6");
            entity.Property(e => e.Pol7).HasColumnName("pol7");
            entity.Property(e => e.Pol8).HasColumnName("pol8");
            entity.Property(e => e.Pol9).HasColumnName("pol9");
            entity.Property(e => e.PolText1).HasColumnName("pol_Text_1");
            entity.Property(e => e.PolText2).HasColumnName("pol_Text_2");
            entity.Property(e => e.PolText3).HasColumnName("pol_Text_3");
            entity.Property(e => e.Polltype).HasColumnName("polltype");
            entity.Property(e => e.Reserveid).HasColumnName("reserveid");
        });

        modelBuilder.Entity<PublicationCdStor>(entity =>
        {
            entity.ToTable("Publication_CD_Stor");

            entity.Property(e => e.Cdid).HasColumnName("CDID");
            entity.Property(e => e.CountCd).HasColumnName("CountCD");
        });

        modelBuilder.Entity<PublicationCdregister>(entity =>
        {
            entity.ToTable("Publication_CDRegister");

            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.Teachername).HasColumnName("teachername");
        });

        modelBuilder.Entity<PublicationCost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Publications_Costs");

            entity.ToTable("Publication_Costs");

            entity.Property(e => e.Published).HasColumnName("published");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationCosts)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Costs_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationFactor>(entity =>
        {
            entity.ToTable("Publication_Factor");

            entity.Property(e => e.Buyer).HasColumnName("buyer");
            entity.Property(e => e.CdDiscount).HasColumnName("cdDiscount");
            entity.Property(e => e.CdDiscountper).HasColumnName("cdDiscountper");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Invoicenumber).HasColumnName("invoicenumber");
            entity.Property(e => e.Istemp).HasColumnName("istemp");
            entity.Property(e => e.Paid).HasColumnName("paid");
            entity.Property(e => e.PayDate).HasColumnName("pay_date");
            entity.Property(e => e.Payment).HasColumnName("payment");
            entity.Property(e => e.Phonenumber).HasColumnName("phonenumber");
            entity.Property(e => e.Remaining).HasColumnName("remaining");
        });

        modelBuilder.Entity<PublicationFactorItem>(entity =>
        {
            entity.ToTable("Publication_Factor_Items");

            entity.Property(e => e.Bookid).HasColumnName("bookid");
            entity.Property(e => e.Booktype).HasColumnName("booktype");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationFactorItems)
                .HasForeignKey(d => d.Bookid)
                .HasConstraintName("FK_Publication_Factor_Items_Publications_RegisterBook");

            entity.HasOne(d => d.Factor).WithMany(p => p.PublicationFactorItems)
                .HasForeignKey(d => d.FactorId)
                .HasConstraintName("FK_Publication_Factor_Items_Publication_Factor");
        });

        modelBuilder.Entity<PublicationIndexing>(entity =>
        {
            entity.ToTable("Publication_Indexing");

            entity.Property(e => e.Indextype)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("indextype");
            entity.Property(e => e.Method).HasColumnName("method");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationIndexings)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publication_Indexing_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationStore>(entity =>
        {
            entity.ToTable("Publication_Store");

            entity.Property(e => e.Bookid).HasColumnName("bookid");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationStores)
                .HasForeignKey(d => d.Bookid)
                .HasConstraintName("FK_Publication_Store_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationVerification>(entity =>
        {
            entity.ToTable("Publication_Verification");

            entity.Property(e => e.Bookid).HasColumnName("bookid");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationVerifications)
                .HasForeignKey(d => d.Bookid)
                .HasConstraintName("FK_Publication_Verification_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationsBookEdit>(entity =>
        {
            entity.ToTable("Publications_BookEdit");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationsBookEdits)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_BookEdit_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationsDesign>(entity =>
        {
            entity.ToTable("Publications_Design");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationsDesigns)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Design_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationsIndexing>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Publications_Indexing");

            entity.HasOne(d => d.Book).WithMany()
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Indexing_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationsPaging>(entity =>
        {
            entity.ToTable("Publications_Paging");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationsPagings)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Paging_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationsPermission>(entity =>
        {
            entity.ToTable("Publications_Permissions");

            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.NumberofNl).HasColumnName("NumberofNL");
            entity.Property(e => e.Shabakdate).HasColumnName("SHABAKDate");
            entity.Property(e => e.Shabakno).HasColumnName("SHABAKNo");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationsPermissions)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Permissions_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationsPrinting>(entity =>
        {
            entity.ToTable("Publications_Printing");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationsPrintings)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_Publications_Printing_Publications_RegisterBook");
        });

        modelBuilder.Entity<PublicationsRegisterAssessor>(entity =>
        {
            entity.ToTable("Publications_RegisterAssessor");

            entity.Property(e => e.Bookname).HasColumnName("bookname");
            entity.Property(e => e.Gid).HasColumnName("gid");
            entity.Property(e => e.Recivedate).HasColumnName("recivedate");
            entity.Property(e => e.Words).HasColumnName("words");
        });

        modelBuilder.Entity<PublicationsRegisterBook>(entity =>
        {
            entity.ToTable("Publications_RegisterBook");

            entity.Property(e => e.Bookname).HasColumnName("bookname");

            entity.HasOne(d => d.Assessor).WithMany(p => p.PublicationsRegisterBooks)
                .HasForeignKey(d => d.AssessorId)
                .HasConstraintName("FK_Publications_RegisterBook_Publications_RegisterAssessor");
        });

        modelBuilder.Entity<PublicationsRepository>(entity =>
        {
            entity.ToTable("Publications_Repository");

            entity.Property(e => e.First).HasColumnName("first");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Book).WithMany(p => p.PublicationsRepositories)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Publications_Repository_Publications_RegisterBook");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Regions_1");
        });

        modelBuilder.Entity<SalaryPardakht>(entity =>
        {
            entity.ToTable("Salary_Pardakht");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Accb).HasColumnName("accb");
            entity.Property(e => e.Bimeh).HasColumnName("bimeh");
            entity.Property(e => e.BimehC).HasColumnName("bimeh_c");
            entity.Property(e => e.BimehCHekmat).HasColumnName("bimeh_c_hekmat");
            entity.Property(e => e.BimehHekmat)
                .HasDefaultValue(0.0, "DF_Salary_Pardakht_bimeh_hekmat")
                .HasColumnName("bimeh_hekmat");
            entity.Property(e => e.CodeMarhaleh).HasColumnName("Code_marhaleh");
            entity.Property(e => e.DescAll).HasColumnName("descAll");
            entity.Property(e => e.FileMatn).HasColumnName("fileMatn");
            entity.Property(e => e.FundAccount).HasColumnName("Fund_Account");
            entity.Property(e => e.FundAlias).HasColumnName("Fund_Alias");
            entity.Property(e => e.FundBank).HasColumnName("Fund_bank");
            entity.Property(e => e.FundMembersNumber).HasColumnName("fund_members_number");
            entity.Property(e => e.FundName).HasColumnName("Fund_Name");
            entity.Property(e => e.FundTotal).HasColumnName("fund_total");
            entity.Property(e => e.Islock).HasColumnName("islock");
            entity.Property(e => e.Khales).HasColumnName("khales");
            entity.Property(e => e.MablaghHrof).HasColumnName("Mablagh_hrof");
            entity.Property(e => e.Maliat).HasColumnName("maliat");
            entity.Property(e => e.Tedad).HasColumnName("tedad");
            entity.Property(e => e.Vdate).HasColumnName("VDate");

            entity.HasOne(d => d.AccbNavigation).WithMany(p => p.SalaryPardakhts)
                .HasForeignKey(d => d.Accb)
                .HasConstraintName("FK_Salary_Pardakht_T_ACC_Base");
        });

        modelBuilder.Entity<SalaryUserBase>(entity =>
        {
            entity.ToTable("Salary_User_Base");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActiveYear).HasColumnName("active_year");
            entity.Property(e => e.Bid).HasColumnName("BID");
            entity.Property(e => e.Boan).HasColumnName("boan");
            entity.Property(e => e.FarzandTedad).HasColumnName("farzand_tedad");
            entity.Property(e => e.IncomYear).HasColumnName("incom_year");
            entity.Property(e => e.Isgurd).HasColumnName("isgurd");
            entity.Property(e => e.Locked).HasColumnName("locked");
            entity.Property(e => e.Maskan).HasColumnName("maskan");
            entity.Property(e => e.Masooliat2).HasColumnName("Masooliat_2");
            entity.Property(e => e.ModirMablagh).HasColumnName("modir_mablagh");
            entity.Property(e => e.MpayehP).HasColumnName("Mpayeh_p");
            entity.Property(e => e.PayehEmtiaz).HasColumnName("Payeh_emtiaz");
            entity.Property(e => e.PercentEzafeh).HasColumnName("Percent_Ezafeh");
            entity.Property(e => e.PercentModir).HasColumnName("Percent_modir");
            entity.Property(e => e.Refahi).HasColumnName("refahi");
            entity.Property(e => e.Roshdsanavat).HasColumnName("roshdsanavat");
            entity.Property(e => e.SHesab).HasColumnName("S_Hesab");
            entity.Property(e => e.Savabeghsanavat).HasColumnName("savabeghsanavat");
            entity.Property(e => e.Tahsilat1).HasColumnName("tahsilat1");
            entity.Property(e => e.Tahsilat2).HasColumnName("tahsilat2");
            entity.Property(e => e.Tahsilat3).HasColumnName("tahsilat3");
            entity.Property(e => e.Tahsilat4).HasColumnName("tahsilat4");
            entity.Property(e => e.Uid).HasColumnName("UID");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.SalaryUserBases)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("FK_Salary_User_Base_T_ACC_Base");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.SalaryUserBases)
                .HasForeignKey(d => d.Uid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Salary_User_Base_T_Users");
        });

        modelBuilder.Entity<SalaryUserList>(entity =>
        {
            entity.ToTable("Salary_User_List");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ActiveYear).HasColumnName("active_year");
            entity.Property(e => e.Bid).HasColumnName("BID");
            entity.Property(e => e.Bimdateend).HasColumnName("bimdateend");
            entity.Property(e => e.Bimdatestar).HasColumnName("bimdatestar");
            entity.Property(e => e.BimehCHekmat).HasColumnName("bimeh_c_hekmat");
            entity.Property(e => e.Boan).HasColumnName("boan");
            entity.Property(e => e.DateTo).HasColumnName("dateTo");
            entity.Property(e => e.Datefrom).HasColumnName("datefrom");
            entity.Property(e => e.Describ).HasColumnName("describ");
            entity.Property(e => e.FundLoanPay).HasColumnName("fund_loan_pay");
            entity.Property(e => e.FundMonthly).HasColumnName("fund_Monthly");
            entity.Property(e => e.Hagholad).HasColumnName("hagholad");
            entity.Property(e => e.IncomYear).HasColumnName("incom_year");
            entity.Property(e => e.Islock).HasColumnName("islock");
            entity.Property(e => e.MablaghBedehkari).HasColumnName("Mablagh_Bedehkari");
            entity.Property(e => e.MablaghBestankari).HasColumnName("Mablagh_Bestankari");
            entity.Property(e => e.MablaghBimeh).HasColumnName("Mablagh_bimeh");
            entity.Property(e => e.MablaghBimehC).HasColumnName("Mablagh_bimeh_c");
            entity.Property(e => e.MablaghBimehCHekmat).HasColumnName("Mablagh_bimeh_c_hekmat");
            entity.Property(e => e.MablaghBimehHekmat).HasColumnName("Mablagh_bimeh_hekmat");
            entity.Property(e => e.MablaghNahaee).HasColumnName("Mablagh_Nahaee");
            entity.Property(e => e.Maskan).HasColumnName("maskan");
            entity.Property(e => e.ModirePercent).HasColumnName("Modire_percent");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Other).HasColumnName("other");
            entity.Property(e => e.Pid).HasColumnName("PID");
            entity.Property(e => e.Refahi).HasColumnName("refahi");
            entity.Property(e => e.SaatEzafekari).HasColumnName("Saat_ezafekari");
            entity.Property(e => e.SaatKari).HasColumnName("Saat_kari");
            entity.Property(e => e.SaatMamoriat).HasColumnName("Saat_mamoriat");
            entity.Property(e => e.SaatPadash).HasColumnName("Saat_padash");
            entity.Property(e => e.TedadOlad).HasColumnName("tedad_olad");
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.SalaryUserLists)
                .HasForeignKey(d => d.Bid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Salary_User_List_T_Acc_User_Base");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.SalaryUserLists)
                .HasForeignKey(d => d.Pid)
                .HasConstraintName("FK_Salary_User_List_Salary_Pardakht");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.SalaryUserLists)
                .HasForeignKey(d => d.Uid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Salary_User_List_T_Users");
        });

        modelBuilder.Entity<SalaryUserPay>(entity =>
        {
            entity.ToTable("Salary_User_Pay");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Bid).HasColumnName("BID");
            entity.Property(e => e.DateP).HasColumnName("Date_p");
            entity.Property(e => e.MKhales).HasColumnName("M_Khales");
            entity.Property(e => e.MKol).HasColumnName("M_kol");
            entity.Property(e => e.MMaliat)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("M_Maliat");
            entity.Property(e => e.MVahedJ).HasColumnName("M_vahed_J");
            entity.Property(e => e.MVahedS).HasColumnName("M_vahed_s");
            entity.Property(e => e.MaliatPercent).HasColumnName("Maliat_percent");
            entity.Property(e => e.SHesab).HasColumnName("S_Hesab");
            entity.Property(e => e.SanadSh).HasColumnName("Sanad_sh");
            entity.Property(e => e.Uid).HasColumnName("UID");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<SalaryUserPayBase>(entity =>
        {
            entity.ToTable("Salary_User_Pay_Base");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GroupJ).HasColumnName("Group_J");
            entity.Property(e => e.GroupS).HasColumnName("Group_S");
            entity.Property(e => e.HeyatJ).HasColumnName("Heyat_J");
            entity.Property(e => e.HeyatS).HasColumnName("Heyat_S");
            entity.Property(e => e.MaliatPercent).HasColumnName("Maliat_percent");
            entity.Property(e => e.MoshaverehJ).HasColumnName("Moshavereh_J");
            entity.Property(e => e.MoshaverehS).HasColumnName("Moshavereh_S");
            entity.Property(e => e.OstadJ).HasColumnName("Ostad_J");
            entity.Property(e => e.OstadS).HasColumnName("Ostad_S");
        });

        modelBuilder.Entity<SalaryUserPayGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Salary_User_Pay_group_1");

            entity.ToTable("Salary_User_Pay_group");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MablaghJ).HasColumnName("Mablagh_j");
            entity.Property(e => e.MablaghSaat).HasColumnName("Mablagh_saat");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.SalaryUserPayGroups)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("FK_Salary_User_Pay_group_Salary_User_Pay_Base");
        });

        modelBuilder.Entity<SendSm>(entity =>
        {
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Senddate)
                .HasColumnType("datetime")
                .HasColumnName("senddate");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.SendSms)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("FK_SendSms_T_User_Amoozesh");
        });

        modelBuilder.Entity<TAcc>(entity =>
        {
            entity.ToTable("T_ACC");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.InTime)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("In_Time");
            entity.Property(e => e.Jid).HasColumnName("jid");
            entity.Property(e => e.OutTime)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("Out_time");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TAccBase>(entity =>
        {
            entity.ToTable("T_ACC_Base");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Aeleh).HasColumnName("aeleh");
            entity.Property(e => e.BimehAccount).HasColumnName("bimeh_Account");
            entity.Property(e => e.BimehCAccount).HasColumnName("bimeh_c_Account");
            entity.Property(e => e.Bmodiriat).HasColumnName("BModiriat");
            entity.Property(e => e.Boan).HasColumnName("boan");
            entity.Property(e => e.Bsabegheh1).HasColumnName("BSabegheh1");
            entity.Property(e => e.Btaahol).HasColumnName("BTaahol");
            entity.Property(e => e.Codesazman).HasColumnName("codesazman");
            entity.Property(e => e.Codeshobeh).HasColumnName("codeshobeh");
            entity.Property(e => e.Ezafehpercent).HasColumnName("ezafehpercent");
            entity.Property(e => e.Ezafehsaghf).HasColumnName("ezafehsaghf");
            entity.Property(e => e.Farzand).HasColumnName("farzand");
            entity.Property(e => e.FundAccount).HasColumnName("Fund_Account");
            entity.Property(e => e.FundAlias).HasColumnName("Fund_Alias");
            entity.Property(e => e.FundBank).HasColumnName("Fund_bank");
            entity.Property(e => e.FundName).HasColumnName("Fund_Name");
            entity.Property(e => e.LastDate).HasColumnName("last_date");
            entity.Property(e => e.LastStep).HasColumnName("last_Step");
            entity.Property(e => e.Locked)
                .HasComment("Locking")
                .HasColumnName("locked");
            entity.Property(e => e.Mahaneh).HasColumnName("mahaneh");
            entity.Property(e => e.Maliat).HasColumnName("maliat");
            entity.Property(e => e.Maskan).HasColumnName("maskan");
            entity.Property(e => e.Masooliatmozaf).HasColumnName("masooliatmozaf");
            entity.Property(e => e.MinimumMonthlyFee).HasColumnName("minimum_monthly_fee");
            entity.Property(e => e.MinimumStartFee).HasColumnName("minimum_start_fee");
            entity.Property(e => e.ModirPercent).HasColumnName("modir_percent");
            entity.Property(e => e.Refahi).HasColumnName("refahi");
            entity.Property(e => e.Region).HasColumnName("region");
            entity.Property(e => e.Roshdsanavat).HasColumnName("roshdsanavat");
            entity.Property(e => e.Tahsilat1).HasColumnName("tahsilat1");
            entity.Property(e => e.Tahsilat2).HasColumnName("tahsilat2");
            entity.Property(e => e.Tahsilat3).HasColumnName("tahsilat3");
            entity.Property(e => e.Tahsilat4).HasColumnName("tahsilat4");
            entity.Property(e => e.Tatilat).HasColumnName("tatilat");

            entity.HasOne(d => d.RegionNavigation).WithMany(p => p.TAccBases)
                .HasForeignKey(d => d.Region)
                .HasConstraintName("FK_T_ACC_Base_Regions");
        });

        modelBuilder.Entity<TAccFactor>(entity =>
        {
            entity.ToTable("T_Acc_Factor");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CostCode).HasDefaultValue(1, "DF_T_Acc_Factor_CostCode");
            entity.Property(e => e.Describ).HasColumnName("describ");
            entity.Property(e => e.Mablagh).HasColumnName("mablagh");
            entity.Property(e => e.Pdate).HasMaxLength(15);
            entity.Property(e => e.Regionid).HasColumnName("regionid");
            entity.Property(e => e.SId).HasColumnName("S_ID");
            entity.Property(e => e.Tempdate1).HasColumnName("tempdate1");
            entity.Property(e => e.Tempdate2).HasColumnName("tempdate2");

            entity.HasOne(d => d.CostCodeNavigation).WithMany(p => p.TAccFactors)
                .HasForeignKey(d => d.CostCode)
                .HasConstraintName("FK_T_Acc_Factor_CostsCode");

            entity.HasOne(d => d.Region).WithMany(p => p.TAccFactors)
                .HasForeignKey(d => d.Regionid)
                .HasConstraintName("FK_T_Acc_Factor_Regions");
        });

        modelBuilder.Entity<TAccUserBase>(entity =>
        {
            entity.ToTable("T_Acc_User_Base");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Fmonth).HasColumnName("FMonth");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TAccounting>(entity =>
        {
            entity.ToTable("T_Accounting");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AccountNo)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.BankAmel)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.CardNo)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.SumMablagh).HasColumnName("Sum_Mablagh");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TAmoozeshDoreh>(entity =>
        {
            entity.ToTable("T_amoozesh_doreh");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Ayam).HasColumnName("ayam");
            entity.Property(e => e.DateEnd).HasColumnName("Date_end");
            entity.Property(e => e.DateStart).HasColumnName("Date_start");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.F2).HasColumnName("f2");
            entity.Property(e => e.F3).HasColumnName("f3");
            entity.Property(e => e.Jensiat).HasColumnName("jensiat");
            entity.Property(e => e.OstadId).HasColumnName("OstadID");
            entity.Property(e => e.Saat).HasColumnName("saat");
            entity.Property(e => e.Sal).HasColumnName("sal");
            entity.Property(e => e.Sharayet).HasColumnName("sharayet");
            entity.Property(e => e.Shomareh).HasColumnName("shomareh");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Region).WithMany(p => p.TAmoozeshDorehs)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_amoozesh_doreh_Regions");

            entity.HasOne(d => d.Teacher).WithMany(p => p.TAmoozeshDorehs)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK_T_amoozesh_doreh_Users_2");
        });

        modelBuilder.Entity<TAmoozeshDorehStage>(entity =>
        {
            entity.ToTable("T_amoozesh_doreh_stage");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Descrip).HasColumnName("descrip");
            entity.Property(e => e.DorehId).HasColumnName("doreh_id");
            entity.Property(e => e.TedadHazer).HasColumnName("tedad_hazer");
            entity.Property(e => e.TedadJaleseh).HasColumnName("tedad_jaleseh");
            entity.Property(e => e.TimeIn).HasColumnName("time_in");
            entity.Property(e => e.TimeOut).HasColumnName("time_out");

            entity.HasOne(d => d.Doreh).WithMany(p => p.TAmoozeshDorehStages)
                .HasForeignKey(d => d.DorehId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_amoozesh_doreh_stage_T_amoozesh_doreh");
        });

        modelBuilder.Entity<TAmoozeshUserDoreh>(entity =>
        {
            entity.ToTable("T_Amoozesh_user_doreh");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DorehId).HasColumnName("doreh_id");
            entity.Property(e => e.Enseraf).HasColumnName("enseraf");
            entity.Property(e => e.Tscode).HasColumnName("tscode");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Doreh).WithMany(p => p.TAmoozeshUserDorehs)
                .HasForeignKey(d => d.DorehId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_Amoozesh_user_doreh_T_amoozesh_doreh");

            entity.HasOne(d => d.User).WithMany(p => p.TAmoozeshUserDorehs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_Amoozesh_user_doreh_T_User_Amoozesh");
        });

        modelBuilder.Entity<TAmoozeshUserHozor>(entity =>
        {
            entity.ToTable("T_Amoozesh_user_hozor");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descrip).HasColumnName("descrip");
            entity.Property(e => e.Stage).HasColumnName("stage");
            entity.Property(e => e.TimeIn).HasColumnName("time_in");
            entity.Property(e => e.TimeOut).HasColumnName("time_out");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.StageNavigation).WithMany(p => p.TAmoozeshUserHozors)
                .HasForeignKey(d => d.Stage)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_Amoozesh_user_hozor_T_amoozesh_doreh_stage");

            entity.HasOne(d => d.User).WithMany(p => p.TAmoozeshUserHozors)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_Amoozesh_user_hozor_T_User_Amoozesh");
        });

        modelBuilder.Entity<TBanksForm>(entity =>
        {
            entity.ToTable("T_Banks_form");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Datep).HasColumnName("datep");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.F2).HasColumnName("f2");
            entity.Property(e => e.F3).HasColumnName("f3");
            entity.Property(e => e.Kmarhaleh).HasColumnName("kmarhaleh");
            entity.Property(e => e.Ksazman).HasColumnName("ksazman");
            entity.Property(e => e.Kshobeh).HasColumnName("kshobeh");
            entity.Property(e => e.Nameshobeh).HasColumnName("nameshobeh");
            entity.Property(e => e.Varasi).HasColumnName("varasi");
        });

        modelBuilder.Entity<TEshterakUser>(entity =>
        {
            entity.ToTable("T_Eshterak_Users");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.Codeposti)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Credit).HasColumnName("credit");
            entity.Property(e => e.Eshtaerakno)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("eshtaerakno");
            entity.Property(e => e.Fame)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Lname)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Tel)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("tel");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<TFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_T_Files_1");

            entity.ToTable("T_Files");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Date2).HasColumnType("datetime");
            entity.Property(e => e.FileName)
                .IsUnicode(false)
                .HasColumnName("File_name");
            entity.Property(e => e.FileType)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("File_type");
            entity.Property(e => e.Fname).HasColumnName("fname");
            entity.Property(e => e.Lname).HasColumnName("lname");
            entity.Property(e => e.Other).HasColumnName("other");
            entity.Property(e => e.SelfFile)
                .HasColumnType("image")
                .HasColumnName("Self_File");
            entity.Property(e => e.Tedadsafheh).HasColumnName("tedadsafheh");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Zaban).HasColumnName("zaban");
        });

        modelBuilder.Entity<TFiles1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_T_Files");

            entity.ToTable("T_Files_1");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Date2).HasColumnType("datetime");
            entity.Property(e => e.FileName)
                .IsUnicode(false)
                .UseCollation("Persian_100_CI_AS")
                .HasColumnName("File_name");
            entity.Property(e => e.FileType)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("File_type");
            entity.Property(e => e.Fname).HasColumnName("fname");
            entity.Property(e => e.Lname).HasColumnName("lname");
            entity.Property(e => e.Other).HasColumnName("other");
            entity.Property(e => e.SelfFile)
                .HasColumnType("image")
                .HasColumnName("Self_File");
            entity.Property(e => e.Tedadsafheh).HasColumnName("tedadsafheh");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Zaban).HasColumnName("zaban");
        });

        modelBuilder.Entity<TKari>(entity =>
        {
            entity.ToTable("T_Kari");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateIn).HasColumnName("Date_in");
            entity.Property(e => e.DateOut).HasColumnName("Date_out");
            entity.Property(e => e.Mahal)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Mozoo).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.NFaaliat).HasColumnName("N_faaliat");
            entity.Property(e => e.Semat)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS");

            entity.HasOne(d => d.User).WithMany(p => p.TKaris)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_T_Kari_T_Users");
        });

        modelBuilder.Entity<TMoshavereTime>(entity =>
        {
            entity.ToTable("T_Moshavere_time");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateSave).HasMaxLength(50);
            entity.Property(e => e.Decrip).HasColumnName("decrip");
            entity.Property(e => e.Final).HasColumnName("final");
            entity.Property(e => e.Maghta).HasColumnName("maghta");
            entity.Property(e => e.OstdaRahnama).HasColumnName("OStdaRahnama");
            entity.Property(e => e.Soalasli).HasColumnName("soalasli");
            entity.Property(e => e.UserId)
                .HasMaxLength(12)
                .IsFixedLength()
                .HasColumnName("UserID");
            entity.Property(e => e.ZamanPishnahadi).HasColumnName("zamanPishnahadi");
            entity.Property(e => e.Zamineh).HasColumnName("zamineh");
        });

        modelBuilder.Entity<TMoshavereh>(entity =>
        {
            entity.ToTable("T_Moshavereh");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Jdat)
                .HasColumnType("datetime")
                .HasColumnName("jdat");
            entity.Property(e => e.Kholaseh).HasColumnName("kholaseh");
            entity.Property(e => e.Modat).HasColumnName("modat");
            entity.Property(e => e.MoshaverId).HasColumnName("MoshaverID");
            entity.Property(e => e.Ndate)
                .HasColumnType("datetime")
                .HasColumnName("ndate");
            entity.Property(e => e.Next).HasColumnName("next");
            entity.Property(e => e.Nobat).HasColumnName("nobat");
            entity.Property(e => e.Ntime).HasColumnName("ntime");
            entity.Property(e => e.Rahkar).HasColumnName("rahkar");
            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Tozihat).HasColumnName("tozihat");
            entity.Property(e => e.Zin).HasColumnName("zin");
            entity.Property(e => e.Zout).HasColumnName("zout");
        });

        modelBuilder.Entity<TMoshaverehSabt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_T_Moshavereh_sabt_1");

            entity.ToTable("T_Moshavereh_sabt");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Jalesehdate)
                .HasColumnType("datetime")
                .HasColumnName("jalesehdate");
            entity.Property(e => e.Ostadid).HasColumnName("ostadid");
            entity.Property(e => e.Ostadmail).HasColumnName("ostadmail");
            entity.Property(e => e.Requestid).HasColumnName("requestid");
            entity.Property(e => e.Requestmail).HasColumnName("requestmail");
            entity.Property(e => e.Saat).HasColumnName("saat");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<TPayan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_payan");

            entity.ToTable("T_payan");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AlloPrint).HasColumnName("allo_print");
            entity.Property(e => e.AlloShow).HasColumnName("allo_show");
            entity.Property(e => e.Chekideh).HasColumnName("chekideh");
            entity.Property(e => e.D1).HasColumnName("d1");
            entity.Property(e => e.DateDefa).HasColumnName("Date_defa");
            entity.Property(e => e.DateSabt).HasColumnName("date_sabt");
            entity.Property(e => e.Davar1).HasColumnName("davar_1");
            entity.Property(e => e.Davar2).HasColumnName("davar_2");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.Gerayesh).HasColumnName("gerayesh");
            entity.Property(e => e.Lang).HasColumnName("lang");
            entity.Property(e => e.MTahsil).HasColumnName("m_tahsil");
            entity.Property(e => e.Mozoo).HasColumnName("mozoo");
            entity.Property(e => e.NegahdariP).HasColumnName("negahdari_p");
            entity.Property(e => e.Negarandeh).HasColumnName("negarandeh");
            entity.Property(e => e.OsMoshaver).HasColumnName("os_moshaver");
            entity.Property(e => e.OsRahnama).HasColumnName("os_rahnama");
            entity.Property(e => e.Sath).HasColumnName("sath");
            entity.Property(e => e.Sex).HasColumnName("sex");
            entity.Property(e => e.Tanzimname).HasColumnName("tanzimname");
            entity.Property(e => e.Tozihat).HasColumnName("tozihat");
        });

        modelBuilder.Entity<TPayanCombo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("t_payan_combo");

            entity.Property(e => e.MTahsil).HasColumnName("m_tahsil");
        });

        modelBuilder.Entity<TPayanCounter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Payan_counter");

            entity.ToTable("T_Payan_counter");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<TSalary>(entity =>
        {
            entity.ToTable("T_Salary");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateTo).HasColumnName("dateTo");
            entity.Property(e => e.Datefrom).HasColumnName("datefrom");
            entity.Property(e => e.Describ).HasColumnName("describ");
            entity.Property(e => e.Masooliatmozaf).HasColumnName("masooliatmozaf");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Sabegheh).HasColumnName("sabegheh");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<TTalif>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Talif");

            entity.ToTable("T_Talif");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Ktype).HasColumnName("KType");
            entity.Property(e => e.Mozo).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Nasher).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Onvan).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.TTalifs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_T_Talif_T_Users");
        });

        modelBuilder.Entity<TUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_T_Users_1");

            entity.ToTable("T_Users");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AkMadrak).HasColumnName("AK_madrak");
            entity.Property(e => e.DateEnd)
                .HasColumnType("datetime")
                .HasColumnName("Date_end");
            entity.Property(e => e.DateStart)
                .HasColumnType("datetime")
                .HasColumnName("Date_start");
            entity.Property(e => e.Descripton).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.F2).HasColumnName("f2");
            entity.Property(e => e.Faname)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("FAname");
            entity.Property(e => e.Fname)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.HamkariVaziat).HasColumnName("Hamkari_vaziat");
            entity.Property(e => e.KodeMelli)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("Kode_melli");
            entity.Property(e => e.Lname)
                .HasMaxLength(30)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.MahalSodorshenasname)
                .HasMaxLength(30)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("mahal_sodorshenasname");
            entity.Property(e => e.MahalTavalod)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("Mahal_Tavalod");
            entity.Property(e => e.Masoliat).HasColumnName("masoliat");
            entity.Property(e => e.Masooliatmozaf).HasColumnName("masooliatmozaf");
            entity.Property(e => e.NHamkari)
                .HasMaxLength(25)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("N_hamkari");
            entity.Property(e => e.NOzviat)
                .HasMaxLength(15)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("N_ozviat");
            entity.Property(e => e.OnvanPayan3)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("Onvan_Payan3");
            entity.Property(e => e.OnvanPayan4)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("Onvan_Payan4");
            entity.Property(e => e.RotbehElmi)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("Rotbeh_elmi");
            entity.Property(e => e.SHesab).HasColumnName("S_hesab");
            entity.Property(e => e.SOzviat)
                .HasMaxLength(50)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("S_ozviat");
            entity.Property(e => e.SOzviatketabkhaneh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("S_ozviatketabkhaneh");
            entity.Property(e => e.SParvandehMarkazModiriat)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("S_parvandeh_markaz_modiriat");
            entity.Property(e => e.SShenasnameh)
                .HasMaxLength(10)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("S_shenasnameh");
            entity.Property(e => e.Sabegheh).HasColumnName("sabegheh");
            entity.Property(e => e.TOzviat)
                .HasColumnType("datetime")
                .HasColumnName("T_ozviat");
            entity.Property(e => e.TVorodHozeh).HasColumnName("T_vorod_Hozeh");
            entity.Property(e => e.Tabeiat)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.TaeedOzviatKetabkhaneh).HasColumnName("taeedOzviatKetabkhaneh");
            entity.Property(e => e.TahsilatHozew).HasColumnName("Tahsilat_hozew");
            entity.Property(e => e.Tasvir).HasColumnType("image");
            entity.Property(e => e.UserId)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("UserID");
            entity.Property(e => e.Username)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS");
            entity.Property(e => e.Userpass)
                .HasMaxLength(20)
                .UseCollation("Albanian_CI_AS");
        });

        modelBuilder.Entity<TUserAmoozesh>(entity =>
        {
            entity.ToTable("T_User_Amoozesh");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cm)
                .HasMaxLength(10)
                .HasColumnName("cm");
            entity.Property(e => e.DateSabet).HasColumnName("date_sabet");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Enseraf).HasColumnName("enseraf");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.F2).HasColumnName("f2");
            entity.Property(e => e.F3).HasColumnName("f3");
            entity.Property(e => e.Faname).HasColumnName("faname");
            entity.Property(e => e.Fname).HasColumnName("fname");
            entity.Property(e => e.Hide).HasColumnName("hide");
            entity.Property(e => e.Internet).HasColumnName("internet");
            entity.Property(e => e.Isview).HasColumnName("isview");
            entity.Property(e => e.Jensiat).HasColumnName("jensiat");
            entity.Property(e => e.Keshvar).HasColumnName("keshvar");
            entity.Property(e => e.Lasteditdate).HasColumnName("lasteditdate");
            entity.Property(e => e.Lname).HasColumnName("lname");
            entity.Property(e => e.MahalSodor).HasColumnName("Mahal_sodor");
            entity.Property(e => e.MahalTavalod).HasColumnName("Mahal_Tavalod");
            entity.Property(e => e.MajmaaOldCode).HasColumnName("majmaa_old_code");
            entity.Property(e => e.Registerdate).HasColumnName("registerdate");
            entity.Property(e => e.Reshteh).HasColumnName("reshteh");
            entity.Property(e => e.SCode).HasColumnName("s_Code");
            entity.Property(e => e.SMarkaz).HasColumnName("s_markaz");
            entity.Property(e => e.Shoghl).HasColumnName("shoghl");
            entity.Property(e => e.Ss).HasColumnName("ss");
            entity.Property(e => e.THozeh).HasColumnName("T_hozeh");
            entity.Property(e => e.TKelasic).HasColumnName("T_kelasic");
            entity.Property(e => e.Tabeeat).HasColumnName("tabeeat");
            entity.Property(e => e.Tahol).HasColumnName("tahol");
            entity.Property(e => e.Tel1).HasColumnName("tel_1");
            entity.Property(e => e.Tel2).HasColumnName("Tel_2");
            entity.Property(e => e.Tscode).HasColumnName("tscode");
            entity.Property(e => e.Tt).HasColumnName("tt");

            entity.HasOne(d => d.Region).WithMany(p => p.TUserAmoozeshes)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("FK_T_User_Amoozesh_Regions");
        });

        modelBuilder.Entity<TUserMoshavereh>(entity =>
        {
            entity.ToTable("T_User_moshavereh");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Akarinmadark).HasColumnName("akarinmadark");
            entity.Property(e => e.F1).HasColumnName("f1");
            entity.Property(e => e.Faname)
                .HasMaxLength(20)
                .HasColumnName("FAname");
            entity.Property(e => e.Fname).HasMaxLength(20);
            entity.Property(e => e.InFinal).HasColumnName("in_final");
            entity.Property(e => e.KodeMelli)
                .HasMaxLength(20)
                .HasColumnName("Kode_melli");
            entity.Property(e => e.Lname).HasMaxLength(30);
            entity.Property(e => e.MahalSodorshenasname)
                .HasMaxLength(30)
                .HasColumnName("mahal_sodorshenasname");
            entity.Property(e => e.MahalTavalod)
                .HasMaxLength(20)
                .HasColumnName("Mahal_Tavalod");
            entity.Property(e => e.MoshaverPayan3).HasColumnName("Moshaver_payan3");
            entity.Property(e => e.MoshaverPayan4).HasColumnName("Moshaver_payan4");
            entity.Property(e => e.OnvanPayan3)
                .HasMaxLength(80)
                .HasColumnName("Onvan_Payan3");
            entity.Property(e => e.OnvanPayan4)
                .HasMaxLength(80)
                .HasColumnName("Onvan_Payan4");
            entity.Property(e => e.OstadPayan3).HasColumnName("Ostad_payan3");
            entity.Property(e => e.OstadPayan4).HasColumnName("Ostad_payan4");
            entity.Property(e => e.SShenasnameh)
                .HasMaxLength(10)
                .HasColumnName("S_shenasnameh");
            entity.Property(e => e.Saat1).HasColumnName("saat1");
            entity.Property(e => e.Saat2).HasColumnName("saat2");
            entity.Property(e => e.Saat3).HasColumnName("saat3");
            entity.Property(e => e.TahsilatHozew).HasColumnName("Tahsilat_hozew");
            entity.Property(e => e.Tarikhtavalod).HasColumnType("datetime");
            entity.Property(e => e.TelKar).HasColumnName("Tel_Kar");
            entity.Property(e => e.Username).HasMaxLength(20);
            entity.Property(e => e.Userpass).HasMaxLength(20);
            entity.Property(e => e.Zaman1).HasColumnName("zaman1");
            entity.Property(e => e.Zaman2).HasColumnName("zaman2");
            entity.Property(e => e.Zaman3).HasColumnName("zaman3");
        });

        modelBuilder.Entity<TUserSemat>(entity =>
        {
            entity.ToTable("T_User_semat");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SematId).HasColumnName("SematID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Taddress>(entity =>
        {
            entity.ToTable("TAddress");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CodePosti)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("Code_posti");
            entity.Property(e => e.Email1)
                .HasMaxLength(100)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("email1");
            entity.Property(e => e.Email2)
                .HasMaxLength(100)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("email2");
            entity.Property(e => e.Faddress)
                .HasMaxLength(300)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("FAddress");
            entity.Property(e => e.Fax1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("fax1");
            entity.Property(e => e.Fax2)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("fax2");
            entity.Property(e => e.Ftype).HasColumnName("FType");
            entity.Property(e => e.Mobile1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("mobile1");
            entity.Property(e => e.Mobile2)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("mobile2");
            entity.Property(e => e.Tel1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("tel1");
            entity.Property(e => e.Tel2)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("tel2");
            entity.Property(e => e.Website1)
                .HasMaxLength(100)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("website1");
            entity.Property(e => e.Website2)
                .HasMaxLength(100)
                .UseCollation("Albanian_CI_AS")
                .HasColumnName("website2");

            entity.HasOne(d => d.User).WithMany(p => p.Taddresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TAddress_T_Users");
        });

        modelBuilder.Entity<TeacherCourse>(entity =>
        {
            entity.ToTable("teacherCourse");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PCourseName)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("pCourseName");
            entity.Property(e => e.PHours)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("pHours");
            entity.Property(e => e.PId).HasColumnName("pID");
            entity.Property(e => e.PPlace)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("pPlace");
            entity.Property(e => e.TId).HasColumnName("tID");
            entity.Property(e => e.TName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("tName");
            entity.Property(e => e.TPic)
                .HasColumnType("image")
                .HasColumnName("tPic");
        });

        modelBuilder.Entity<TeacherInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TeacherInfo1");

            entity.ToTable("TeacherInfo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bd)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("bd");
            entity.Property(e => e.CourseTeached).HasColumnName("courseTeached");
            entity.Property(e => e.CourseTeaching).HasColumnName("courseTeaching");
            entity.Property(e => e.Faname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("faname");
            entity.Property(e => e.Fd)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("fd");
            entity.Property(e => e.Fname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fname");
            entity.Property(e => e.Hadd).HasColumnName("hadd");
            entity.Property(e => e.Htel)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("htel");
            entity.Property(e => e.Lname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("lname");
            entity.Property(e => e.Mtel)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("mtel");
            entity.Property(e => e.Pic)
                .HasColumnType("image")
                .HasColumnName("pic");
            entity.Property(e => e.Ssn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ssn");
            entity.Property(e => e.Wadd).HasColumnName("wadd");
            entity.Property(e => e.Wtel)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("wtel");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("units");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Leaveparent).HasColumnName("leaveparent");
            entity.Property(e => e.Mid).HasColumnName("mid");
            entity.Property(e => e.Missionparent)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("missionparent");
            entity.Property(e => e.Parent).HasColumnName("parent");
            entity.Property(e => e.Unitname).HasColumnName("unitname");

            entity.HasOne(d => d.ParentNavigation).WithMany(p => p.InverseParentNavigation)
                .HasForeignKey(d => d.Parent)
                .HasConstraintName("FK_units_units");
        });

        modelBuilder.Entity<UnitsUser>(entity =>
        {
            entity.ToTable("units_users");

            entity.HasIndex(e => e.Uid, "IX_units_users").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Unitid).HasColumnName("unitid");

            entity.HasOne(d => d.Unit).WithMany(p => p.UnitsUsers)
                .HasForeignKey(d => d.Unitid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_units_users_units");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRoles_Roles");
        });

        modelBuilder.Entity<Userregion>(entity =>
        {
            entity.HasOne(d => d.Region).WithMany(p => p.Userregions)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Userregions_Regions");

            entity.HasOne(d => d.User).WithMany(p => p.Userregions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Userregions_T_Users");
        });

        modelBuilder.Entity<Users2>(entity =>
        {
            entity.ToTable("Users_2");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AKar).HasColumnName("A_Kar");
            entity.Property(e => e.AManzel).HasColumnName("A_Manzel");
            entity.Property(e => e.Bank).HasColumnName("bank");
            entity.Property(e => e.BankShobeh).HasColumnName("bank_shobeh");
            entity.Property(e => e.BankType).HasColumnName("bank_type");
            entity.Property(e => e.Cm).HasColumnName("CM");
            entity.Property(e => e.ControlType).HasColumnName("Control_Type");
            entity.Property(e => e.Faname).HasColumnName("FAname");
            entity.Property(e => e.HamkariNahweh).HasColumnName("hamkari_nahweh");
            entity.Property(e => e.HamkariType).HasColumnName("hamkari_Type");
            entity.Property(e => e.KarType).HasColumnName("Kar_Type");
            entity.Property(e => e.LMadrak).HasColumnName("L_madrak");
            entity.Property(e => e.MTahsil).HasColumnName("M_Tahsil");
            entity.Property(e => e.MTavalod).HasColumnName("M_tavalod");
            entity.Property(e => e.MahalKar).HasColumnName("Mahal_kar");
            entity.Property(e => e.Region).HasColumnName("region");
            entity.Property(e => e.Shahr).HasColumnName("shahr");
            entity.Property(e => e.Ss).HasColumnName("SS");
            entity.Property(e => e.THowzeh).HasColumnName("T_Howzeh");
            entity.Property(e => e.TKelasic).HasColumnName("T_Kelasic");
            entity.Property(e => e.TelKar).HasColumnName("Tel_Kar");
            entity.Property(e => e.TelManzel).HasColumnName("Tel_manzel");
            entity.Property(e => e.TelMobile).HasColumnName("Tel_mobile");
            entity.Property(e => e.Tt).HasColumnName("TT");
            entity.Property(e => e.Uid).HasColumnName("UID");

            entity.HasOne(d => d.RegionNavigation).WithMany(p => p.Users2s)
                .HasForeignKey(d => d.Region)
                .HasConstraintName("FK_Users_2_Regions");
        });

        modelBuilder.Entity<UsersMoshavereh>(entity =>
        {
            entity.ToTable("Users_Moshavereh");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Cm).HasColumnName("cm");
            entity.Property(e => e.ControlType).HasColumnName("Control_type");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Internet).HasColumnName("internet");
            entity.Property(e => e.Isview).HasColumnName("isview");
            entity.Property(e => e.Jensiat).HasColumnName("jensiat");
            entity.Property(e => e.Lasteditdate).HasColumnName("lasteditdate");
            entity.Property(e => e.MSdor).HasColumnName("M_sdor");
            entity.Property(e => e.MTahsil).HasColumnName("M_Tahsil");
            entity.Property(e => e.MTavalod).HasColumnName("M_tavalod");
            entity.Property(e => e.PanelId).HasColumnName("PanelID");
            entity.Property(e => e.Registerdate).HasColumnName("registerdate");
            entity.Property(e => e.Ss).HasColumnName("ss");
            entity.Property(e => e.THowzeh).HasColumnName("T_Howzeh");
            entity.Property(e => e.TKelasic).HasColumnName("T_kelasic");
            entity.Property(e => e.Tahol).HasColumnName("tahol");
            entity.Property(e => e.Tell1).HasColumnName("Tell_1");
            entity.Property(e => e.Tell2).HasColumnName("Tell_2");
            entity.Property(e => e.Tell3).HasColumnName("Tell_3");
            entity.Property(e => e.Tt).HasColumnName("tt");
            entity.Property(e => e.Uid).HasColumnName("UID");

            entity.HasOne(d => d.Region).WithMany(p => p.UsersMoshaverehs)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("FK_Users_Moshavereh_Regions");
        });

        modelBuilder.Entity<UsersMoshaverehRequest>(entity =>
        {
            entity.ToTable("Users_Moshavereh_Request");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ControlType).HasColumnName("Control_type");
            entity.Property(e => e.DateRequest).HasColumnName("date_request");
            entity.Property(e => e.DateSabt).HasColumnName("Date_Sabt");
            entity.Property(e => e.Internet).HasColumnName("internet");
            entity.Property(e => e.Isview).HasColumnName("isview");
            entity.Property(e => e.Maghta).HasColumnName("maghta");
            entity.Property(e => e.Mozoo).HasColumnName("mozoo");
            entity.Property(e => e.OMoshaver).HasColumnName("o_Moshaver");
            entity.Property(e => e.ORahnama).HasColumnName("o_rahnama");
            entity.Property(e => e.PcodeRequest).HasColumnName("Pcode_request");
            entity.Property(e => e.RegDate).HasColumnType("datetime");
            entity.Property(e => e.SAsli).HasColumnName("s_asli");
            entity.Property(e => e.Saat1).HasColumnName("saat1");
            entity.Property(e => e.Saat2).HasColumnName("saat2");
            entity.Property(e => e.Saat3).HasColumnName("saat3");
            entity.Property(e => e.Saat4).HasColumnName("saat4");
            entity.Property(e => e.Saat5).HasColumnName("saat5");
            entity.Property(e => e.Saat6).HasColumnName("saat6");
            entity.Property(e => e.TopId).HasColumnName("TopID");
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.UsersMoshaverehRequests)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK_Users_Moshavereh_Request_Users_Moshavereh");
        });

        modelBuilder.Entity<UsersMoshaverehRequestMeet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Users_Moshavereh_Request_ostad");

            entity.ToTable("Users_Moshavereh_Request_Meet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ControlType).HasColumnName("Control_type");
            entity.Property(e => e.Isdell).HasColumnName("isdell");
            entity.Property(e => e.Isshow).HasColumnName("isshow");
            entity.Property(e => e.Kholaseh).HasColumnName("kholaseh");
            entity.Property(e => e.Modat).HasColumnName("modat");
            entity.Property(e => e.NextDate).HasColumnName("next_date");
            entity.Property(e => e.NextTime).HasColumnName("next_time");
            entity.Property(e => e.Nobat).HasColumnName("nobat");
            entity.Property(e => e.Ostadid).HasColumnName("ostadid");
            entity.Property(e => e.PaydateEn).HasColumnName("paydate_en");
            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.Rahkar).HasColumnName("rahkar");
            entity.Property(e => e.Reqid).HasColumnName("reqid");
            entity.Property(e => e.StudRecivDate).HasColumnName("stud_reciv_date");
            entity.Property(e => e.StudSendDate).HasColumnName("stud_send_date");
            entity.Property(e => e.TarikhEn).HasColumnName("tarikh_en");
            entity.Property(e => e.TeachRecivDate).HasColumnName("teach_reciv_date");
            entity.Property(e => e.TeachSendDate).HasColumnName("teach_send_date");
            entity.Property(e => e.TimeIn).HasColumnName("time_in");
            entity.Property(e => e.TimeOut).HasColumnName("time_out");
            entity.Property(e => e.Tozihat).HasColumnName("tozihat");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Wordcount).HasColumnName("wordcount");

            entity.HasOne(d => d.Ostad).WithMany(p => p.UsersMoshaverehRequestMeets)
                .HasForeignKey(d => d.Ostadid)
                .HasConstraintName("FK_Users_Moshavereh_Request_Meet_Users_2");

            entity.HasOne(d => d.Req).WithMany(p => p.UsersMoshaverehRequestMeets)
                .HasForeignKey(d => d.Reqid)
                .HasConstraintName("FK_Users_Moshavereh_Request_Meet_Users_Moshavereh");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.UsersMoshaverehRequestMeets)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK_Users_Moshavereh_Request_Meet_Users_Moshavereh1");
        });

        modelBuilder.Entity<UsersMoshaverehRequestMeetPoll>(entity =>
        {
            entity.ToTable("Users_Moshavereh_Request_Meet_Poll");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Ostadid).HasColumnName("ostadid");
            entity.Property(e => e.Pol1).HasColumnName("pol1");
            entity.Property(e => e.Pol10).HasColumnName("pol10");
            entity.Property(e => e.Pol11).HasColumnName("pol11");
            entity.Property(e => e.Pol12).HasColumnName("pol12");
            entity.Property(e => e.Pol13).HasColumnName("pol13");
            entity.Property(e => e.Pol14).HasColumnName("pol14");
            entity.Property(e => e.Pol15).HasColumnName("pol15");
            entity.Property(e => e.Pol16).HasColumnName("pol16");
            entity.Property(e => e.Pol2).HasColumnName("pol2");
            entity.Property(e => e.Pol3).HasColumnName("pol3");
            entity.Property(e => e.Pol4).HasColumnName("pol4");
            entity.Property(e => e.Pol5).HasColumnName("pol5");
            entity.Property(e => e.Pol6).HasColumnName("pol6");
            entity.Property(e => e.Pol7).HasColumnName("pol7");
            entity.Property(e => e.Pol8).HasColumnName("pol8");
            entity.Property(e => e.Pol9).HasColumnName("pol9");
            entity.Property(e => e.PolText1).HasColumnName("pol_Text_1");
            entity.Property(e => e.PolText2).HasColumnName("pol_Text_2");
            entity.Property(e => e.PolText3).HasColumnName("pol_Text_3");
            entity.Property(e => e.Polltype).HasColumnName("polltype");
            entity.Property(e => e.Reserveid).HasColumnName("reserveid");

            entity.HasOne(d => d.Ostad).WithMany(p => p.UsersMoshaverehRequestMeetPolls)
                .HasForeignKey(d => d.Ostadid)
                .HasConstraintName("FK_Users_Moshavereh_Request_Meet_Poll_Users_2");
        });

        modelBuilder.Entity<Zaban>(entity =>
        {
            entity.ToTable("Zaban");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Describ).UseCollation("Albanian_CI_AS");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.ZabanName).HasColumnName("Zaban_Name");

            entity.HasOne(d => d.User).WithMany(p => p.Zabans)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Zaban_T_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
