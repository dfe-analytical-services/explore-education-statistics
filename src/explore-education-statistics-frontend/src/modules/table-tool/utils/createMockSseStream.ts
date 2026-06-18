import { TtSearchStreamMessage } from '@frontend/services/tableToolSearchService';

export default function createMockSseStream() {
  const encoder = new TextEncoder();

  // Create standard SSE formatted chunks
  const formatSse = (data: TtSearchStreamMessage) =>
    `data: ${JSON.stringify(data)}\n\n`;

  return new ReadableStream({
    async start(controller) {
      const delay = (ms: number) =>
        new Promise(resolve => setTimeout(resolve, ms));

      try {
        // Stage 1: Starting
        controller.enqueue(
          encoder.encode(formatSse({ stage: 'starting pipeline' })),
        );
        await delay(2000); // Wait 1 second

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
        await delay(3000);

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
                shortlistedDatasets: [
                  {
                    fileId: '688db31c-66bd-4dbd-b73c-08dec0963904',
                    title: 'Reasons for absence and attendance',
                    relevanceReason:
                      'This dataset provides local authority level data on reasons for pupil absence, including holidays, with weekly time frames that cover the last 4 weeks, making it directly relevant to the query for Sheffield.',
                    relevantFilters: ['Attendance reason', 'Time frame'],
                    relevanceScore: 73.1,
                  },
                ],
                confidence: 'high',
              },
            }),
          ),
        );
        await delay(6000);

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
                          property: 'LocalAuthority',
                          name: 'Sheffield',
                          score: 100,
                        },
                      ],
                    },
                    aiSummary:
                      'This data is relevant because This dataset provides local authority level data on reasons for pupil absence, including holidays, with weekly time frames that cover the last 4 weeks, making it directly relevant to the query for Sheffield.\n It contains information about Daily and weekly local authority, regional and national reasons for pupil attendance and absence. Figures are provided for state-funded primary, secondary and special schools.',
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
