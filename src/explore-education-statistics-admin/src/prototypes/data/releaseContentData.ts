import { ReleaseContent } from '@admin/services/releaseContentService';

const headlines = `In total there were 26,955 new entrants to ITT in 2023/24 compared to 28,463 in 2022/23 [1], 36,159 in 2021/22 and 40,377 in 2020/21.  
In 2020/21, we saw an unprecedented increase in new entrants to ITT, which was likely to be a direct result of the impact of COVID-19, and these higher levels 
continued, to a lesser extent, into 2021/22. In 2022/23 and 2023/24, numbers have been below pre-pandemic levels.</p>
<p>Of the new entrants in 2023/24, 21,946 were starting postgraduate ITT, a decrease (3%) from 22,673 in 2022/23. 
There were 5,009 new entrants to undergraduate ITT in 2023/24, a decrease (13%) from 5,790 in 2022/23.</p>
<p>The percentage of the Postgraduate Initial Teacher Training (PGITT) target achieved for all subjects (secondary and primary) was 62%. 
This is a decrease of 8 percentage points from 70% in 2022/23. This was driven by a decrease in the number of new entrants to PGITT (of 727) 
and an increase in the target (from 32,600 in 2022/23 to 35,540 in 2023/24).`;

const content1 = `<p>This statistical release provides provisional figures on the number of new entrants who have started an initial teacher training 
(ITT) programme in England in 2023/24 by school subject, training route, training region and a range of trainee demographic factors. This statistical 
release includes revised data for 2022/23.&nbsp;</p><p>These statistics cover those training to teach via both postgraduate and undergraduate routes, 
as well as a separate section on those undertaking Early Years Initial Teacher Training (EYITT).&nbsp;</p><p>The following tables are included:&nbsp;</p>
<ul><li>national tables for the training years 2019/20 to 2023/24 by route, phase, subject, region and trainee characteristics (main postgraduate and 
  undergraduate routes)&nbsp;</li><li>provider-level tables for the training years 2019/20 to 2023/24 by route, phase and subject (main postgraduate and 
    undergraduate routes)&nbsp;</li><li>a national table for the training years 2019/20 to 
2023/24 by route and trainee characteristics (EYITT route).&nbsp;</li></ul><p>The number of new entrants who have started postgraduate ITT is compared 
to the Department's annual postgraduate ITT trainee targets. PGITT targets are selected using analysis from the Teacher Workforce Model (TWM). 
<a href="#_ftnref5">[5]</a>&nbsp;</p><p>The ITT Census publication was produced using data extracted from the Register Trainee Teachers service, including 
data collected in the HESA (Higher Education Statistics Agency) ITT collection. Please see methodology for further detail. &nbsp;</p><p>This year the 
publication includes end of application cycle data on the number of candidates making applications to ITT courses, and the number of acceptances to ITT 
courses, for courses that start in the years 2022/23 and 2023/24. These statistics are classified as official statistics in development, which is a 
temporary label for new statistics that are undergoing development and testing. More information can be found under Background to end of application cycle 
statistics.</p>`;

const prototypeReleaseContent: ReleaseContent = {
  release: {
    approvalStatus: 'Draft',
    content: [
      {
        id: 'content-section-1-id',
        heading: 'About these statistics',
        order: 0,
        content: [
          {
            body: content1,
            comments: [],
            id: 'content-block-1',
            order: 0,
            type: 'HtmlBlock',
          },
        ],
      },
      {
        id: 'content-section-2-id',
        heading: 'New entrants to postgraduate ITT by nationality',
        order: 1,
        content: [
          {
            body: content1,
            comments: [],
            id: 'content-block-3',
            order: 0,
            type: 'HtmlBlock',
          },
        ],
      },
    ],
    coverageTitle: 'Academic year',
    downloadFiles: [
      {
        id: 'download-file-id',
        extension: 'csv',
        fileName: 'File name',
        name: 'Download file name',
        size: '100kb',
        type: 'DataZip',
      },
    ],
    hasDataGuidance: true,
    hasPreReleaseAccessList: false,
    headlinesSection: {
      id: 'headlines-section-id',
      heading: 'Headlines section heading',
      order: 0,
      content: [
        {
          body: headlines,
          comments: [],
          id: 'headlines-block',
          order: 0,
          type: 'HtmlBlock',
        },
      ],
    },
    id: 'Release-title-id',
    keyStatistics: [
      {
        id: 'key-statistic-id',
        statistic: '26,955',
        title: 'Total new entrants to ITT',
        trend: '(down 5% from 28,463 in 2022/23)',
        type: 'KeyStatisticText',
      },
      {
        id: 'key-statistic-id-2',
        statistic: '21,946',
        title: 'Postgraduate total new entrants to ITT',
        trend: '(down 3% from 22,673 in 2022/23)',
        type: 'KeyStatisticText',
      },
    ],
    keyStatisticsSecondarySection: {
      id: 'key-statistics-secondary-section-id',
      content: [],
      heading: 'Key statistics secondary section heading',
      order: 0,
    },
    latestRelease: false,
    publicationId: 'publication-id',
    publication: {
      contact: {
        contactName: 'Contact name',
        teamEmail: 'contact@test.com',
        teamName: 'Team name',
      },
      id: 'publication-id',
      methodologies: [
        {
          id: 'methodology-id',
          title: 'Methodology title',
          slug: 'methodology-slug',
        },
      ],
      releaseSeries: [],
      slug: 'publication-slug',
      title: 'Initial Teacher Training Census',
      theme: {
        id: 'test-theme',
        title: 'Test theme',
      },
    },
    relatedInformation: [
      {
        id: 'related-information-id',
        description: 'Related information description',
        url: 'https://test.com',
      },
    ],
    slug: 'Release-title',
    summarySection: {
      id: 'summary-section-id',
      content: [
        {
          body: '<p>National and provider-level information about the numbers and characteristics of new entrants to Initial Teacher Training (ITT) in England in the training year 2023/24; and 2023/24 PGITT targets. The statistical release also includes information on numbers and characteristics of new entrants to early years ITT, and application information for ITT postgraduate courses.</p>',
          comments: [],
          id: 'summary-block-id',
          order: 0,
          type: 'HtmlBlock',
        },
      ],
      heading: 'Summary block heading',
      order: 0,
    },
    title: 'Academic year 2023/34',
    type: 'OfficialStatistics',
    updates: [],
    yearTitle: '2023/34',
  },
  unattachedDataBlocks: [],
};
export default prototypeReleaseContent;
