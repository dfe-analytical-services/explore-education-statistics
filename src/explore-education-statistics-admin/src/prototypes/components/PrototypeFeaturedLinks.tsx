import React from 'react';
import PrototypeGridView from '@admin/prototypes/components/PrototypeGridView';
import PrototypeChevronCard from '@admin/prototypes/components/PrototypeChevronCard';

interface Props {
  dataset?: string;
  publication?: string;
}

const FeaturedLink = ({ dataset, publication }: Props) => {
  return (
    <>
      <PrototypeGridView>
        {publication === 'pub-1' && (
          <>
            {(dataset === 'subject-all' || dataset === 'subject-1') && (
              <PrototypeChevronCard
                title="Starts by age and level"
                url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/24fba78b-86df-4342-359b-08daa20fa891"
                description="Annual apprenticeship starts by age and level"
              />
            )}

            {(dataset === 'subject-all' || dataset === 'subject-2') && (
              <>
                <PrototypeChevronCard
                  title="Rates by sector subject area and Ethnicity group learners"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/5d90dad5-f10f-48e2-b1b0-ebdf1a03642c"
                  description="Apprenticeship achievement rates by sector subject area and Ethnicity group learners"
                />
                <PrototypeChevronCard
                  title="Rates by sector subject area and gender"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/c7997707-e733-4591-b993-3faa4ac3668f"
                  description="All age apprenticeships overall achievement rates demographic summary"
                />
                <PrototypeChevronCard
                  title="Rates by sector subject area and learners with learning difficulty and or disability"
                  url="https://explore-education-statistics.service.gov.uk/"
                  description="Apprenticeship achievement rates by sector subject area and learners with learning difficulty and or disability"
                />
                <PrototypeChevronCard
                  title="Rates demographic summary"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/74fca436-5e3b-4d7e-8ccc-83a78086caee"
                  description="All age apprenticeships overall achievement rates demographic summary"
                />
              </>
            )}

            {(dataset === 'subject-all' || dataset === 'subject-3') && (
              <>
                <PrototypeChevronCard
                  title="Achievements by level"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/dad1704c-2e46-4a2a-8768-61fe94f94c1b"
                  description="Apprenticeship achievements by level, 2015/16 to 2020/21"
                />
                <PrototypeChevronCard
                  title="Rates demographic summary"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/74fca436-5e3b-4d7e-8ccc-83a78086caee"
                  description="All age apprenticeships overall achievement rates demographic summary"
                />
                <PrototypeChevronCard
                  title="Starts and achievements by level, age, and funding type"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/9b554ec1-8639-4b8c-50b9-08daa6e07a35"
                  description="Apprenticeship participation and achievements by age and level, reported to date for 2021/22 with equivalent figures for 2018/19 to 2020/21"
                />
              </>
            )}
          </>
        )}

        {publication === 'pub-3' && (
          <>
            {(dataset === 'subject-all' || dataset === 'subject-1') && (
              <PrototypeChevronCard
                title="Headline attainment data by school type, 2018/19 to 2021/22, England"
                url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/87c4e983-9163-4a6d-a59c-58331454ece8"
                description="The following attainment data broken down by school type: Average Attainment 8 score of all pupils Average EBacc APS score per pupil Percentage of pupils achieving grades 5 or above in English and mathematics GCSEs Percentage of pupils entering the English Baccalaureate"
              />
            )}

            {(dataset === 'subject-all' || dataset === 'subject-2') && (
              <>
                <PrototypeChevronCard
                  title="Characteristics summary"
                  url="/data-tables/fast-track/87c4e983-9163-4a6d-a59c-58331454ece8"
                  description="A summary of major entry and attainment data for pupils of different characteristics in all state-funded schools, 2018/19-2021/22"
                />
                <PrototypeChevronCard
                  title="Headline attainment measures by ethnic minor pupil characteristic, 2018/19 to 2021/22, state-funded schools, England"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/33a093be-9810-4890-a5a4-d0913cd967a0"
                  description="AAttainment for the following measures broken down by ethnic minor: Average Attainment 8 score of all pupils Average EBacc APS score per pupil Percentage of pupils achieving grades 5 or above in English and mathematics GCSEs Percentage of pupils entering the English Baccalaureate"
                />
              </>
            )}

            {(dataset === 'subject-all' || dataset === 'subject-3') && (
              <>
                <PrototypeChevronCard
                  title="Achievements by level"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/dad1704c-2e46-4a2a-8768-61fe94f94c1b"
                  description="Apprenticeship achievements by level, 2015/16 to 2020/21"
                />
                <PrototypeChevronCard
                  title="Rates demographic summary"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/74fca436-5e3b-4d7e-8ccc-83a78086caee"
                  description="All age apprenticeships overall achievement rates demographic summary"
                />
                <PrototypeChevronCard
                  title="Starts and achievements by level, age, and funding type"
                  url="https://explore-education-statistics.service.gov.uk/data-tables/fast-track/9b554ec1-8639-4b8c-50b9-08daa6e07a35"
                  description="Apprenticeship participation and achievements by age and level, reported to date for 2021/22 with equivalent figures for 2018/19 to 2020/21"
                />
              </>
            )}
          </>
        )}
      </PrototypeGridView>
    </>
  );
};

export default FeaturedLink;
