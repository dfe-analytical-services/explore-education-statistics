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
          // Simulates the Wi-Fi dropping or Azure timing out mid-stream.
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
        await delay(300);

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
        await delay(800);

        // Stage 4: Complete
        controller.enqueue(
          encoder.encode(
            formatSse({
              stage: 'pipeline complete',
              data: {
                datasets: [
                  {
                    fileId: '688db31c-66bd-4dbd-b73c-08dec0963904',
                    filters: ['Legacy family holiday (f)'],
                    indicators: ['Percent of sessions'],
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
                    title: 'Reasons for absence and attendance',
                  },
                  {
                    fileId: '10308fbb-da53-4eae-20d2-08dec542d092',
                    filters: ['lorem'],
                    indicators: ['lorem'],
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
                    title: 'Other final result',
                  },
                ],
                token_usage: 3766,
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
