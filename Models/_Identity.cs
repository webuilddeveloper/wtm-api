using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    [BsonIgnoreExtraElements]
    public class Identity
    {
        public Identity()
        {
            status = "";
            code = "";
            sequence = 0;
            titleShort = "";
            titleShortEN = "";
            codeShort = "";

            category = "";
            category2 = "";
            categoryItem = "";
            language = "th";

            title = "";
            subTitle = "";
            description = "";

            titleEN = "";
            subTitleEN = "";
            descriptionEN = "";

            createBy = "system";
            updateBy = "system";
            isActive = false;
            isHighlight = false;
            isPublic = false;
            isNotification = false;
            docDate = DateTime.Now;

            mainPage = false;
            newsPage = false;
            eventPage = false;
            contactPage = false;
            knowledgePage = false;
            knowledgeVetPage = false;
            privilegePage = false;
            poiPage = false;
            pollPage = false;
            suggestionPage = false;
            notificationPage = false;
            reporterPage = false;
            trainingPage = false;
            welfarePage = false;
            warningPage = false;
            fundPage = false;
            cooperativeFormPage = false;
            examinationPage = false;
            imageEventPage = false;
            eventAbroadPage = false;
            lawPage = false;
            trainingInstitutePage = false;
            expertBranchPage = false;
            verifyApprovedUserPage = false;
            seminarPage = false;

            importantPage = false;
            vetEnewsPage = false;
            personnelPage = false;
            personnelStructurePage = false;

            //report
            reportNumberMemberRegisterPage = false;
            reportMemberRegisterPage = false;
            reportNewsCategoryPage = false;
            reportNewsPage = false;
            reportKnowledgeCategoryPage = false;
            reportKnowledgePage = false;
            reportNewsKeysearchPage = false;
            reportKnowledgeKeysearchPage = false;

            reportContactKeysearchPage = false;
            reportEventCalendarKeysearchPage = false;
            reportPrivilegeKeysearchPage = false;
            reportPoiKeysearchPage = false;
            reportPollKeysearchPage = false;
            reportWarningKeysearchPage = false;
            reportWelfareKeysearchPage = false;
            reportReporterKeysearchPage = false;

            reportReporterPage = false;
            reportReporterCreatePage = false;
            reportEventCalendarPage = false;
            reportWarningPage = false;
            reportWelfarePage = false;
            reportPollPage = false;
            reportPoiPage = false;
            reportContactPage = false;
            reportPrivilegePage = false;

            reportAboutUsPage = false;
            reportRotationPage = false;
            reportForceAdsPage = false;
            //Master
            swearWordsPage = false;
            masterVeterinaryPage = false;
            contentKeywordPage = false;
            notificationResultPage = false;
            notificationExamPage = false;

            dashboardPage = false;
            productPage = false;
            employeePage = false;
            workProcessPage = false;

            byPass = false;
            lv0 = "";
            lv1 = "";
            lv2 = "";
            lv3 = "";
            lv4 = "";
            lv5 = "";

            organizationMode = "";

            linkUrl = "";
            textButton = "";
            action = "";
            fileUrl = "";
            platform = "";

            profileCode = "";
            cpCode = "";
            cpTitle = "";
            center = "";
            centerName = "";
            centerNameEN = "";
        
        }

        public ObjectId _id { get; set; }

        public string status { get; set; }
        public string code { get; set; }
        public int sequence { get; set; }
        public string titleShort { get; set; }
        public string titleShortEN { get; set; }
        public string codeShort { get; set; }

        public string category { get; set; }
        public string category2 { get; set; }
        public string categoryItem { get; set; }
        public string language { get; set; }

        public string title { get; set; }
        public string subTitle { get; set; }
        public string description { get; set; }

        public string titleEN { get; set; }
        public string subTitleEN { get; set; }
        public string descriptionEN { get; set; }

        public string createDate { get; set; }
        public string createTime { get; set; }
        public string createBy { get; set; }
        public string updateDate { get; set; }
        public string updateTime { get; set; }
        public string updateBy { get; set; }
        public bool isActive { get; set; }
        public bool isHighlight { get; set; }
        public bool isPublic { get; set; }
        public bool isNotification { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime docDate { get; set; }
        public string docTime { get; set; }

        public bool mainPage { get; set; }
        public bool newsPage { get; set; }
        public bool eventPage { get; set; }
        public bool contactPage { get; set; }
        public bool knowledgePage { get; set; }
        public bool knowledgeVetPage { get; set; }
        public bool privilegePage { get; set; }
        public bool poiPage { get; set; }
        public bool pollPage { get; set; }
        public bool suggestionPage { get; set; }
        public bool notificationPage { get; set; }
        public bool reporterPage { get; set; }
        public bool trainingPage { get; set; }
        public bool welfarePage { get; set; }
        public bool warningPage { get; set; }
        public bool fundPage { get; set; }
        public bool cooperativeFormPage { get; set; }
        public bool examinationPage { get; set; }
        public bool imageEventPage { get; set; }
        public bool eventAbroadPage { get; set; }
        public bool lawPage { get; set; }
        public bool trainingInstitutePage { get; set; }
        public bool expertBranchPage { get; set; }
        public bool verifyApprovedUserPage { get; set; }
        public bool seminarPage { get; set; }

        public bool importantPage { get; set; }
        public bool vetEnewsPage { get; set; }
        public bool personnelPage { get; set; }
        public bool personnelStructurePage { get; set; }

        public bool reportNumberMemberRegisterPage { get; set; }
        public bool reportMemberRegisterPage { get; set; }
        public bool reportNewsCategoryPage { get; set; }
        public bool reportNewsPage { get; set; }
        public bool reportKnowledgeCategoryPage { get; set; }
        public bool reportKnowledgePage { get; set; }
        public bool reportNewsKeysearchPage { get; set; }
        public bool reportKnowledgeKeysearchPage { get; set; }
        public bool reportContactKeysearchPage { get; set; }
        public bool reportEventCalendarKeysearchPage { get; set; }
        public bool reportPrivilegeKeysearchPage { get; set; }
        public bool reportPoiKeysearchPage { get; set; }
        public bool reportPollKeysearchPage { get; set; }
        public bool reportWarningKeysearchPage { get; set; }
        public bool reportWelfareKeysearchPage { get; set; }
        public bool reportReporterKeysearchPage { get; set; }
        public bool reportReporterPage { get; set; }

        public bool reportReporterCreatePage { get; set; }
        public bool reportEventCalendarPage { get; set; }
        public bool reportWarningPage { get; set; }
        public bool reportWelfarePage { get; set; }
        public bool reportPollPage { get; set; }
        public bool reportPoiPage { get; set; }
        public bool reportContactPage { get; set; }
        public bool reportPrivilegePage { get; set; }

        public bool reportAboutUsPage { get; set; }
        public bool reportRotationPage { get; set; }
        public bool reportForceAdsPage { get; set; }

        public bool swearWordsPage { get; set; }
        public bool masterVeterinaryPage { get; set; }
        public bool contentKeywordPage { get; set; }
        public bool notificationResultPage { get; set; }
        public bool notificationExamPage { get; set; }

        public bool dashboardPage { get; set; }
        public bool productPage { get; set; }
        public bool employeePage { get; set; }
        public bool workProcessPage { get; set; }
        public bool portfolioPage { get; set; }
        public bool certificatePage { get; set; }


        public bool byPass { get; set; }
        public string lv0 { get; set; }
        public string lv1 { get; set; }
        public string lv2 { get; set; }
        public string lv3 { get; set; }
        public string lv4 { get; set; }
        public string lv5 { get; set; }

        public List<Category> categoryList { get; set; }
        public List<Category> lv0List { get; set; }
        public List<Category> lv1List { get; set; }
        public List<Category> lv2List { get; set; }
        public List<Category> lv3List { get; set; }
        public List<Category> lv4List { get; set; }
        public List<Category> lv5List { get; set; }

        public List<Organization> organization { get; set; }
        public string organizationMode { get; set; }

        public string fileUrl { get; set; }
        public string linkUrl { get; set; }
        public string textButton { get; set; }
        public string action { get; set; }
        public string platform { get; set; }

        public string profileCode { get; set; }
        public string cpCode { get; set; }
        public string cpTitle { get; set; }
        public string center { get; set; }
        public string centerName { get; set; }
        public string centerNameEN { get; set; }
       
    }
}
