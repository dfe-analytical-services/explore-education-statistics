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
                    dataSetFileId: '52d98825-1e90-4752-acb9-e751d03d0d1d',
                    fileId: '9a0aa599-fdfd-40ad-b73a-08dec0963904',
                    publicationId: '96f418e7-3ddb-4a8c-60dc-08deb7f1c424',
                    publicationSlug: 'pupil-attendance-in-schools-in-england',
                    publicationTitle: 'Pupil attendance in schools in England',
                    releaseSlug: '2026-week-20',
                    releaseVersionId: 'db7aad55-30b2-4d7e-bb45-08dec09602df',
                    rawRelevanceScore: 0.024358974769711494,
                    relevanceScore: 74.3,
                    subjectId: '6806855d-79e6-4abd-ed1f-08dec09638f8',
                    title: 'Persistent absence in schools',
                    description:
                      'Persistent absence measures, year to date only, updated each publication week. Figures are provided at the local authority, regional and national level for state-funded primary, secondary and special schools.',
                  },
                  {
                    dataSetFileId: '9e2601f1-2ff5-43a2-b892-502a7dec9ecf',
                    fileId: '688db31c-66bd-4dbd-b73c-08dec0963904',
                    publicationId: '96f418e7-3ddb-4a8c-60dc-08deb7f1c424',
                    publicationSlug: 'pupil-attendance-in-schools-in-england',
                    publicationTitle: 'Pupil attendance in schools in England',
                    releaseSlug: '2026-week-20',
                    releaseVersionId: 'db7aad55-30b2-4d7e-bb45-08dec09602df',
                    rawRelevanceScore: 0.024462364614009857,
                    relevanceScore: 74.6,
                    subjectId: '10ea4c1e-2124-4da4-ed21-08dec09638f8',
                    title: 'Reasons for absence and attendance',
                    description:
                      'Daily and weekly local authority, regional and national reasons for pupil attendance and absence. Figures are provided for state-funded primary, secondary and special schools.',
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
                shortlistedDatasets: returnResults
                  ? [
                      {
                        fileId: '688db31c-66bd-4dbd-b73c-08dec0963904',
                        title: 'Reasons for absence and attendance',
                        relevanceReason:
                          'This dataset provides local authority level data on reasons for pupil absence, including holidays, with weekly time frames that cover the last 4 weeks, making it directly relevant to the query for Sheffield.',
                        relevantFilters: ['Attendance reason', 'Time frame'],
                        relevanceScore: 73.1,
                      },
                    ]
                  : [],
                confidence: 'high',
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
                    dataSetFileId: '9e2601f1-2ff5-43a2-b892-502a7dec9ecf',
                    fileId: '688db31c-66bd-4dbd-b73c-08dec0963904',
                    publicationId: '96f418e7-3ddb-4a8c-60dc-08deb7f1c424',
                    publicationSlug: 'pupil-attendance-in-schools-in-england',
                    publicationTitle: 'Pupil attendance in schools in England',
                    releaseSlug: '2026-week-20',
                    releaseVersionId: 'db7aad55-30b2-4d7e-bb45-08dec09602df',
                    subjectId: '10ea4c1e-2124-4da4-ed21-08dec09638f8',
                    title: 'Reasons for absence and attendance',
                    description:
                      'Daily and weekly local authority, regional and national reasons for pupil attendance and absence. Figures are provided for state-funded primary, secondary and special schools.',
                    filters: [
                      {
                        id: '45eff781-6dbe-4c06-8c90-795cc17bbf38',
                        label: 'Week',
                      },
                      {
                        id: '6b3e255f-3f19-4609-8167-246beb16706d',
                        label: 'Legacy family holiday (f)',
                      },
                      {
                        id: 'b36a0689-f2c6-4464-95be-d82921199721',
                        label: 'Unauthorised holiday (g)',
                      },
                      {
                        id: '62e7150f-29ae-44d7-be9b-85aca0636094',
                        label: 'Authorised holiday (h)',
                      },
                    ],
                    indicators: [
                      {
                        id: 'ccb4341c-3fbe-47ec-087b-08dec0963a29',
                        label: 'Percent of sessions',
                      },
                      {
                        id: 'eb2ea071-0f13-438c-0879-08dec0963a29',
                        label: 'Reference date',
                      },
                    ],
                    timePeriod: {
                      start: {
                        code: 'W17',
                        year: '2026',
                      },
                      end: {
                        code: 'W20',
                        year: '2026',
                      },
                    },
                    geographicLevels: {
                      'Local authority': [
                        {
                          id: 'bbe3cafc-2c62-42d6-4919-08d93bbc8641',
                          label: 'Sheffield',
                          value: 'E08000019',
                        },
                      ],
                      National: [],
                      Regional: [],
                    },
                    relevanceReason:
                      'This dataset provides daily and weekly local authority level data on reasons for pupil absence, including holidays, and covers the relevant recent time period including the last 4 weeks.',
                  },
                ],
                token_usage: {
                  input: 3766,
                  output: 3800,
                },
                cost: 0.004,
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
