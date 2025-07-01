using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace cms_api.Models
{
    public class RegisterCategory : Identity
    {

        public RegisterCategory()
        {
            createAction = false;
            readAction = false;
            updateAction = false;
            deleteAction = false;
            approveAction = false;

            organizationPage = false;
            userRolePage = false;
            memberPage = false;
            memberMobilePage = false;

            splashPage = false;
            mainPopupPage = false;
            bannerPage = false;
            forceAdsPage = false;
            rotationPage = false;
            partnerPage = false;
            alliancePage = false;
            relateAgencyPage = false;
            officeActivitiesPage = false;

            newsCategoryPage = false;
            eventCategoryPage = false;
            contactCategoryPage = false;
            knowledgeCategoryPage = false;
            knowledgeVetCategoryPage = false;
            privilegeCategoryPage = false;
            poiCategoryPage = false;
            pollCategoryPage = false;
            suggestionCategoryPage = false;
            notificationCategoryPage = false;
            welfareCategoryPage = false;
            trainingCategoryPage = false;
            reporterCategoryPage = false;
            warningCategoryPage = false;
            policyApplicationPage = false;
            policyMarketingPage = false;
            memberMobilePolicyApplicationPage = false;
            memberMobilePolicyMarketingPage = false;
            fundCategoryPage = false;
            cooperativeFormCategoryPage = false;
            examinationCategoryPage = false;
            lawCategoryPage = false;
            trainingInstituteCategoryPage = false;
            expertBranchCategoryPage = false;
            verifyApprovedUserCategoryPage = false;
            seminarCategoryPage = false;

            importantCategoryPage = false;
            imageEventCategoryPage = false;
            eventAbroadCategoryPage = false;
            vetEnewsCategoryPage = false;
            personnelStructureCategoryPage = false;

            websitevisitorPage = false;
            cmsvisitorPage = false;
            aboutCommentPage = false;

            productCategoryPage = false;
            employeeCategoryPage = false;
        }

        public string keySearch { get; set; }
        public int skip { get; set; }
        public int limit { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
       
        public bool createAction { get; set; }
        public bool readAction { get; set; }
        public bool updateAction { get; set; }
        public bool deleteAction { get; set; }
        public bool approveAction { get; set; }

        public bool organizationPage { get; set; }
        public bool userRolePage { get; set; }
        public bool memberPage { get; set; }
        public bool memberMobilePage { get; set; }

        public bool logoPage { get; set; }
        public bool splashPage { get; set; }
        public bool mainPopupPage { get; set; }
        public bool bannerPage { get; set; }
        public bool forceAdsPage { get; set; }
        public bool rotationPage { get; set; }
        public bool partnerPage { get; set; }
        public bool alliancePage { get; set; }
        public bool relateAgencyPage { get; set; }
        public bool officeActivitiesPage { get; set; }

        public bool newsCategoryPage { get; set; }
        public bool eventCategoryPage { get; set; }
        public bool contactCategoryPage { get; set; }
        public bool knowledgeCategoryPage { get; set; }
        public bool knowledgeVetCategoryPage { get; set; }
        public bool privilegeCategoryPage { get; set; }
        public bool poiCategoryPage { get; set; }
        public bool pollCategoryPage { get; set; }
        public bool suggestionCategoryPage { get; set; }
        public bool notificationCategoryPage { get; set; }
        public bool welfareCategoryPage { get; set; }
        public bool trainingCategoryPage { get; set; }
        public bool reporterCategoryPage { get; set; }
        public bool warningCategoryPage { get; set; }
        public bool fundCategoryPage { get; set; }
        public bool cooperativeFormCategoryPage { get; set; }
        public bool examinationCategoryPage { get; set; }
        public bool imageEventCategoryPage { get; set; }
        public bool eventAbroadCategoryPage { get; set; }
        public bool lawCategoryPage { get; set; }
        public bool trainingInstituteCategoryPage { get; set; }
        public bool expertBranchCategoryPage { get; set; }
        public bool verifyApprovedUserCategoryPage { get; set; }
        public bool seminarCategoryPage { get; set; }

        public bool importantCategoryPage { get; set; }
        public bool vetEnewsCategoryPage { get; set; }
        public bool personnelStructureCategoryPage { get; set; }
        public bool personnelStructureCategoryPage2 { get; set; }


        public bool policyApplicationPage { get; set; }
        public bool policyMarketingPage { get; set; }
        public bool memberMobilePolicyApplicationPage { get; set; }
        public bool memberMobilePolicyMarketingPage { get; set; }

        public bool websitevisitorPage { get; set; }
        public bool cmsvisitorPage { get; set; }
        public bool aboutCommentPage { get; set; }

        public bool productCategoryPage { get; set; }
        public bool employeeCategoryPage { get; set; }
        public bool certificateCategoryPage { get; set; }
        public bool portfolioCategoryPage { get; set; }

    }

}