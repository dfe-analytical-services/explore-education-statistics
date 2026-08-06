import { TtSearchStreamMessage } from '@frontend/services/tableToolSearchService';

export default function createMockSseStream({
  returnResults = true,
  testErrorType = 'none', // 'none' | 'fatal' | 'retriable'
}) {
  const encoder = new TextEncoder();

  // Create standard SSE formatted chunks
  const formatSse = (data: TtSearchStreamMessage) =>
    `data: ${JSON.stringify(data)}\n\n`;

  return new ReadableStream({
    async start(controller) {
      const delay = (ms: number) =>
        new Promise(resolve => setTimeout(resolve, ms));

      try {
        // Simulate errors if wanted
        if (testErrorType === 'fatal') {
          // Simulates the AI pipeline crashing
          controller.enqueue(
            encoder.encode(
              'event: FatalError\ndata: "Simulated AI pipeline error"\n\n',
            ),
          );
          controller.close();
          return;
        }

        if (testErrorType === 'retriable') {
          // Simulates internet connection failure or other temporary failure.
          controller.close();
          return;
        }

        // Stage 1: Starting
        controller.enqueue(
          encoder.encode(formatSse({ stage: 'starting pipeline' })),
        );
        await delay(100);

        // Stage 2: Retrieved
        controller.enqueue(
          encoder.encode(
            formatSse({
              stage: 'retrieved datasets',
              data: {
                datasets: [
                  {
                    dataSetFileId: 'a207f2ef-0261-46af-a267-5c3c2249c03f',
                    fileId: '8e85f2ab-44ce-4129-f6f9-08dedb7abb40',
                    publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                    publicationSlug: 'further-education-outcomes',
                    publicationTitle: 'Further education outcomes',
                    releaseSlug: '2022-23',
                    releaseVersionId: 'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                    subjectId: '7e340ebf-e6a4-4c5d-2f19-08dedb7abb36',
                    title:
                      'National destinations of FE and Skills learners by demographics (NAT01)',
                    description:
                      'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                    filters: [
                      'Age Group',
                      'Apprenticeship type',
                      'Benefit learner',
                      'Ethnicity',
                      'Learning difficulties',
                      'Level of learning',
                      'Provision',
                      'Sex',
                      'T-Level and Large AGQ',
                    ],
                    indicators: [
                      'Advanced apprenticeship (Level 3 or above)',
                      'Any learning rate',
                      'Below Level 2 (excluding Essential skills)',
                      'Community learning',
                      'Destination not sustained',
                      'Destination not sustained and in receipt of benefits',
                      'Destination not sustained and not on benefits',
                      'English, Maths & ESOL',
                      'Essential Skills',
                      'Full Level 2',
                      'Full Level 3',
                      'In receipt of benefits only',
                      'Intermediate apprenticeship (Level 2)',
                      'Learners',
                      'Level 2',
                      'Level 3',
                      'Level 4/5',
                      'Level not assigned',
                      'Lower quartile annualised earnings',
                      'Median annualised earnings',
                      'No activity captured in data',
                      'Number of learners matched to LEO data',
                      'Number of learners with earnings',
                      'Self-employed rate',
                      'Sustained further education rate',
                      'Sustained apprenticeship rate',
                      'Sustained employment and learning',
                      'Sustained employment only rate',
                      'Sustained employment rate',
                      'Sustained higher education rate (Level 6+)',
                      'Sustained learning only rate',
                      'Sustained learning rate',
                      'Sustained positive destination rate',
                      'Sustained progression from achieved aim',
                      'T Level',
                      'T Level foundation year',
                      'Upper quartile annualised earnings',
                    ],
                    timePeriodRange: {
                      from: 'Academic year 2018',
                      to: 'Academic year 2022',
                    },
                    rawRelevanceScore: 0.02348197251558304,
                    relevanceScore: 71.6,
                  },
                  {
                    dataSetFileId: 'b967e496-16cd-4699-b591-584c40299cf5',
                    fileId: '1232a50e-4f25-4983-f6fb-08dedb7abb40',
                    publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                    publicationSlug: 'further-education-outcomes',
                    publicationTitle: 'Further education outcomes',
                    releaseSlug: '2022-23',
                    releaseVersionId: 'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                    subjectId: '83e127dd-77ca-4cc1-2f1b-08dedb7abb36',
                    title: 'Qualification level destinations (QUA01)',
                    description:
                      'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                    filters: [
                      'Access to Higher Education',
                      'Age Group',
                      'Apprenticeship type',
                      'Level of learning',
                      'Provision',
                      'Qualification title',
                      'Sector subject area tier 2',
                      'T-Level and Large AGQ',
                    ],
                    indicators: [
                      'Advanced apprenticeship (Level 3 or above)',
                      'Any learning rate',
                      'Below Level 2 (excluding Essential skills)',
                      'Community learning',
                      'Destination not sustained',
                      'Destination not sustained and in receipt of benefits',
                      'Destination not sustained and not on benefits',
                      'English, Maths & ESOL',
                      'Essential Skills',
                      'Full Level 2',
                      'Full Level 3',
                      'In receipt of benefits only',
                      'Intermediate apprenticeship (Level 2)',
                      'Learners',
                      'Level 2',
                      'Level 3',
                      'Level 4/5',
                      'Level not assigned',
                      'Lower quartile annualised earnings',
                      'Median annualised earnings',
                      'No activity captured in data',
                      'Number of learners matched to LEO data',
                      'Number of learners with earnings',
                      'Self-employed rate',
                      'Sustained further education rate',
                      'Sustained apprenticeship rate',
                      'Sustained employment and learning',
                      'Sustained employment only rate',
                      'Sustained employment rate',
                      'Sustained higher education rate (Level 6+)',
                      'Sustained learning only rate',
                      'Sustained learning rate',
                      'Sustained positive destination rate',
                      'Sustained progression from achieved aim',
                      'T Level',
                      'T Level foundation year',
                      'Upper quartile annualised earnings',
                    ],
                    timePeriodRange: {
                      from: 'Academic year 2018',
                      to: 'Academic year 2022',
                    },
                    rawRelevanceScore: 0.02500000223517418,
                    relevanceScore: 76.3,
                  },
                ],
              },
            }),
          ),
        );
        await delay(500);

        // Stage 3: Reranker
        controller.enqueue(
          encoder.encode(
            formatSse({
              stage: 'reranker complete',
              data: {
                queryRequirements: {
                  filters: ['Percentage of pupils reported as on holiday'],
                  geography: ['Sheffield'],
                  timePeriod: 'Week 9 2026 to Week 6 2026',
                },
                datasets: returnResults
                  ? [
                      {
                        dataSetFileId: 'a207f2ef-0261-46af-a267-5c3c2249c03f',
                        fileId: '8e85f2ab-44ce-4129-f6f9-08dedb7abb40',
                        publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                        publicationSlug: 'further-education-outcomes',
                        publicationTitle: 'Further education outcomes',
                        releaseSlug: '2022-23',
                        releaseVersionId:
                          'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                        subjectId: '7e340ebf-e6a4-4c5d-2f19-08dedb7abb36',
                        title:
                          'National destinations of FE and Skills learners by demographics (NAT01)',
                        description:
                          'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                        relevanceReason:
                          'This dataset includes data on apprenticeship type and level of learning for FE learners, which directly supports analysis of advanced apprenticeships by level of learning in England.',
                        relevantFilters: [
                          'Apprenticeship type',
                          'Level of learning',
                        ],
                        relevanceScore: 71.6,
                      },
                      {
                        dataSetFileId: 'b967e496-16cd-4699-b591-584c40299cf5',
                        fileId: '1232a50e-4f25-4983-f6fb-08dedb7abb40',
                        publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                        publicationSlug: 'further-education-outcomes',
                        publicationTitle: 'Further education outcomes',
                        releaseSlug: '2022-23',
                        releaseVersionId:
                          'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                        subjectId: '83e127dd-77ca-4cc1-2f1b-08dedb7abb36',
                        title: 'Qualification level destinations (QUA01)',
                        description:
                          'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                        relevanceReason:
                          'This dataset also contains apprenticeship type and level of learning filters, enabling detailed analysis of advanced apprenticeships by level of learning in England.',
                        relevantFilters: [
                          'Apprenticeship type',
                          'Level of learning',
                        ],
                        relevanceScore: 76.3,
                      },
                    ]
                  : [],
                confidence: 'high',
                tokenUsage: { input: 985, output: 280 },
                cost: 0.000786,
              },
            }),
          ),
        );
        await delay(1200);

        // Stage 4: Complete
        controller.enqueue(
          encoder.encode(
            formatSse({
              stage: 'pipeline complete',
              data: {
                datasets: [
                  {
                    dataSetFileId: 'b967e496-16cd-4699-b591-584c40299cf5',
                    fileId: '1232a50e-4f25-4983-f6fb-08dedb7abb40',
                    publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                    publicationSlug: 'further-education-outcomes',
                    publicationTitle: 'Further education outcomes',
                    releaseSlug: '2022-23',
                    releaseVersionId: 'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                    subjectId: '83e127dd-77ca-4cc1-2f1b-08dedb7abb36',
                    title: 'Qualification level destinations (QUA01)',
                    description:
                      'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                    filters: [
                      {
                        id: '092df73f-92a7-459b-b11c-968320503619',
                        label: 'Advanced Apprenticeship',
                      },
                    ],
                    indicators: [],
                    timePeriod: null,
                    geographicLevels: {
                      National: [
                        {
                          id: '376f9a26-dc39-4db3-bb19-0549e59d322a',
                          label: 'England',
                          value: 'E92000001',
                        },
                      ],
                    },
                    relevanceReason:
                      'This dataset also contains apprenticeship type and level of learning filters, enabling detailed breakdowns of advanced apprenticeships by level of learning for England.',
                  },
                  {
                    dataSetFileId: 'a207f2ef-0261-46af-a267-5c3c2249c03f',
                    fileId: '8e85f2ab-44ce-4129-f6f9-08dedb7abb40',
                    publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                    publicationSlug: 'further-education-outcomes',
                    publicationTitle: 'Further education outcomes',
                    releaseSlug: '2022-23',
                    releaseVersionId: 'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                    subjectId: '7e340ebf-e6a4-4c5d-2f19-08dedb7abb36',
                    title:
                      'National destinations of FE and Skills learners by demographics (NAT01)',
                    description:
                      'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                    filters: [],
                    indicators: [
                      {
                        id: '9a618d23-a775-4fc3-156f-08dedb7abc91',
                        label: 'Advanced apprenticeship (Level 3 or above)',
                      },
                      {
                        id: 'e65ea899-188f-43a4-1567-08dedb7abc91',
                        label: 'Full Level 3',
                      },
                      {
                        id: '56d8daf8-7593-483b-1557-08dedb7abc91',
                        label: 'Learners',
                      },
                      {
                        id: '0442ac03-4b51-4555-1566-08dedb7abc91',
                        label: 'Level 3',
                      },
                      {
                        id: 'a04f4a1f-ae84-4c30-1568-08dedb7abc91',
                        label: 'Level 4/5',
                      },
                    ],
                    timePeriod: {
                      start: { code: 'AY', year: 2018 },
                      end: { code: 'AY', year: 2022 },
                    },
                    geographicLevels: {
                      National: [
                        {
                          id: '376f9a26-dc39-4db3-bb19-0549e59d322a',
                          label: 'England',
                          value: 'E92000001',
                        },
                      ],
                    },
                    relevanceReason:
                      'This dataset includes data on apprenticeship type and level of learning, which directly supports analysis of advanced apprenticeships by level of learning in England.',
                  },
                  {
                    dataSetFileId: 'a207f2ef-0261-46af-a267-5c3c2249c03f',
                    fileId: '8e85f2ab-44ce-4129-f6f9-08dedb7abb40',
                    publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                    publicationSlug: 'further-education-outcomes',
                    publicationTitle: 'Further education outcomes',
                    releaseSlug: '2022-23',
                    releaseVersionId: 'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                    subjectId: '7e340ebf-e6a4-4c5d-2f19-08dedb7abb36',
                    title:
                      'National destinations of FE and Skills learners by demographics (NAT01)',
                    description:
                      'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                    filters: [
                      {
                        id: '766921c5-3fbc-46f7-b059-deb2f26df296',
                        label: 'Advanced Apprenticeship',
                      },
                    ],
                    indicators: [],
                    timePeriod: null,
                    geographicLevels: {
                      National: [
                        {
                          id: '376f9a26-dc39-4db3-bb19-0549e59d322a',
                          label: 'England',
                          value: 'E92000001',
                        },
                      ],
                    },
                    relevanceReason:
                      'This dataset includes data on apprenticeship type and level of learning, which directly supports analysis of advanced apprenticeships by level of learning in England.',
                  },
                  {
                    dataSetFileId: 'b967e496-16cd-4699-b591-584c40299cf5',
                    fileId: '1232a50e-4f25-4983-f6fb-08dedb7abb40',
                    publicationId: '83f43ee2-4d3c-420d-b62c-08dedb7678d9',
                    publicationSlug: 'further-education-outcomes',
                    publicationTitle: 'Further education outcomes',
                    releaseSlug: '2022-23',
                    releaseVersionId: 'cdb1156a-8d58-494e-0b82-08dedb771f7c',
                    subjectId: '83e127dd-77ca-4cc1-2f1b-08dedb7abb36',
                    title: 'Qualification level destinations (QUA01)',
                    description:
                      'Reports on the employment and learning destinations, and earnings of all age 16+ FE learners that achieved their aim and were recorded in the ILR. Broken down by provision, learner demographics and level of learning.',
                    filters: [],
                    indicators: [
                      {
                        id: '2162c2a5-8e70-47c7-c924-08dedb7abca0',
                        label: 'Advanced apprenticeship (Level 3 or above)',
                      },
                      {
                        id: '011719a5-c9a0-42a9-c919-08dedb7abca0',
                        label: 'Level 2',
                      },
                      {
                        id: 'ff1f1eb5-c2c5-4e3c-c91b-08dedb7abca0',
                        label: 'Level 3',
                      },
                      {
                        id: 'c7e4d833-a5af-4296-c91d-08dedb7abca0',
                        label: 'Level 4/5',
                      },
                    ],
                    timePeriod: {
                      start: { code: 'AY', year: 2018 },
                      end: { code: 'AY', year: 2022 },
                    },
                    geographicLevels: {
                      National: [
                        {
                          id: '376f9a26-dc39-4db3-bb19-0549e59d322a',
                          label: 'England',
                          value: 'E92000001',
                        },
                      ],
                    },
                    relevanceReason:
                      'This dataset also contains apprenticeship type and level of learning filters, enabling detailed breakdowns of advanced apprenticeships by level of learning for England.',
                  },
                ],
                tokenUsage: { input: 6077, output: 6007 },
                cost: 0.0108406,
              },
            }),
          ),
        );

        // Close the stream normally
        controller.close();
      } catch (e) {
        controller.error(e);
      }
    },
  });
}
