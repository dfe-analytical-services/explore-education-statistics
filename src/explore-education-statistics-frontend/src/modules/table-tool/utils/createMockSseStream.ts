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
                    title: 'Persistent absence in schools',
                    relevanceScore: 50.8,
                    rawRelevanceScore: 0.01666666753590107,
                  },
                  {
                    title: 'Reasons for absence and attendance',
                    relevanceScore: 73.1,
                    rawRelevanceScore: 0.02395833283662796,
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
                    fileId: '688db31c-66bd-4dbd-b73c-08dec0963904',
                    filters: [
                      {
                        id: '6fe1e32e-ec17-478b-904d-11c121f6817b',
                        label: 'State-funded AP school',
                      },
                      {
                        id: '1736fa44-d7af-421c-91a1-69c3b3aed8d9',
                        label: 'State-funded secondary',
                      },
                    ],
                    indicators: [
                      {
                        id: 'ad93c0c6-6485-4817-2fc5-08debbdc383d',
                        label: 'Headcount',
                      },
                    ],
                    geographicLevels: {
                      'Local authority': [
                        {
                          id: 'bbe3cafc-2c62-42d6-4919-08d93bbc8641',
                          label: 'Sheffield',
                          value: 'E08000019',
                        },
                      ],
                    },
                    aiSummary:
                      'This data is relevant because This dataset provides local authority level data on reasons for pupil absence, including holidays, with weekly time frames that cover the last 4 weeks, making it directly relevant to the query for Sheffield.\n It contains information about Daily and weekly local authority, regional and national reasons for pupil attendance and absence. Figures are provided for state-funded primary, secondary and special schools.',
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
                    title: 'Reasons for absence and attendance',
                  },
                  {
                    fileId: '10308fbb-da53-4eae-20d2-08dec542d092',
                    filters: [
                      {
                        id: 'filter-id',
                        label: 'filter label',
                      },
                    ],
                    indicators: [
                      {
                        id: 'indicator-id',
                        label: 'indicator label',
                      },
                    ],
                    geographicLevels: {
                      'Local authority': [
                        {
                          id: 'bbe3cafc-2c62-42d6-4919-08d93bbc8641',
                          label: 'Sheffield',
                          value: 'E08000019',
                        },
                      ],
                    },
                    aiSummary: 'Mock AI summary',
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
                    title: 'Test final result',
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
