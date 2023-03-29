import createPermalinkTable from '@frontend/modules/api/permalink/createPermalinkTable';
import { createApiMocks } from '@frontend-test/apiRouteMocks';

describe('permalink api route', () => {
  test('returns a 400 when there is no body on the request', async () => {
    const { req, res } = createApiMocks({
      method: 'POST',
    });

    await createPermalinkTable(req, res);

    expect(res.statusCode).toBe(400);
    // eslint-disable-next-line no-underscore-dangle
    expect(res._getData()).toEqual({
      message: 'fullTable and configuration required',
      status: 400,
    });
  });

  test('returns a 400 when the body does not contain fullTable and configuration', async () => {
    const { req, res } = createApiMocks({
      method: 'POST',
      body: {
        something: 'else',
      },
    });

    await createPermalinkTable(req, res);

    expect(res.statusCode).toBe(400);
    // eslint-disable-next-line no-underscore-dangle
    expect(res._getData()).toEqual({
      message: 'fullTable and configuration required',
      status: 400,
    });
  });

  test('returns a 500 when cannot build a table from the fullTable and configuration', async () => {
    const { req, res } = createApiMocks({
      method: 'POST',
      body: {
        fullTable: ['not table data'],
        configuration: {},
      },
    });

    await createPermalinkTable(req, res);

    expect(res.statusCode).toBe(500);
  });

  test('returns the table json and title when a valid request is sent', async () => {
    const { req, res } = createApiMocks({
      method: 'POST',
      body: {
        configuration: {
          tableHeaders: {
            columnGroups: [],
            columns: [
              {
                value: '2006_AY',
                type: 'TimePeriod',
              },
            ],
            rowGroups: [],
            rows: [
              {
                value: '1b09726d-00d0-42ee-81a5-59d07c7dd9ba',
                type: 'Indicator',
              },
            ],
          },
        },
        fullTable: {
          subjectMeta: {
            filters: {
              DurationOfFixedPeriodExclusions: {
                id: '4c195395-d76a-4152-a90e-90e42310928d',
                hint: 'Filter by duration of exclusion',
                legend: 'Duration of fixed period exclusions',
                options: {
                  Default: {
                    id: '153e3856-3716-4df8-89ce-24b432296149',
                    label: 'Default',
                    options: [
                      {
                        label: 'Total',
                        value: '22f4e671-2a07-4354-b811-f7bd0d3d55b7',
                      },
                    ],
                    order: 0,
                  },
                },
                name: 'duration_days_fixed_exclusions',
                totalValue: '22f4e671-2a07-4354-b811-f7bd0d3d55b7',
                order: 0,
              },
              SchoolType: {
                id: 'fc2ec4a9-cd4c-4534-8a5d-e4fee75d9215',
                hint: 'Filter by school type',
                legend: 'School type',
                options: {
                  Default: {
                    id: '0f01f49b-280c-40d6-af71-fa96dd2ed6f3',
                    label: 'Default',
                    options: [
                      {
                        label: 'Total',
                        value: '8428ded9-781e-4af6-adfc-628b12489696',
                      },
                    ],
                    order: 0,
                  },
                },
                name: 'school_type',
                totalValue: '8428ded9-781e-4af6-adfc-628b12489696',
                order: 1,
              },
            },
            indicators: [
              {
                label: 'Number of fixed period exclusions',
                unit: '',
                value: '1b09726d-00d0-42ee-81a5-59d07c7dd9ba',
                name: 'num_fixed_excl',
              },
            ],
            locations: {
              country: [
                {
                  id: '376f9a26-dc39-4db3-bb19-0549e59d322a',
                  label: 'England',
                  value: 'E92000001',
                },
              ],
            },
            boundaryLevels: [
              {
                id: 1,
                label:
                  'Countries December 2017 Ultra Generalised Clipped Boundaries in UK',
              },
            ],
            publicationName: 'Permanent and fixed-period exclusions in England',
            subjectName: 'Duration of fixed exclusions',
            timePeriodRange: [
              {
                code: 'AY',
                label: '2006/07',
                year: 2006,
              },
            ],
            geoJsonAvailable: true,
          },
          results: [
            {
              id: '3fccc4bc-ccc6-427a-ba47-fa4174fb6967',
              filters: [
                '8428ded9-781e-4af6-adfc-628b12489696',
                '22f4e671-2a07-4354-b811-f7bd0d3d55b7',
              ],
              geographicLevel: 'country',
              locationId: '376f9a26-dc39-4db3-bb19-0549e59d322a',
              measures: {
                '1b09726d-00d0-42ee-81a5-59d07c7dd9ba': '425590',
              },
              timePeriod: '2006_AY',
            },
          ],
        },
      },
    });

    await createPermalinkTable(req, res);

    const expectedTable = {
      thead: [
        [
          {
            colSpan: 1,
            rowSpan: 1,
            tag: 'td',
          },
          {
            colSpan: 1,
            rowSpan: 1,
            scope: 'col',
            text: '2006/07',
            tag: 'th',
          },
        ],
      ],
      tbody: [
        [
          {
            rowSpan: 1,
            colSpan: 1,
            scope: 'row',
            text: 'Number of fixed period exclusions',
            tag: 'th',
          },
          {
            tag: 'td',
            text: '425,590',
          },
        ],
      ],
    };

    expect(res.statusCode).toBe(200);
    // eslint-disable-next-line no-underscore-dangle
    expect(res._getData()).toEqual({
      table: expectedTable,
      title:
        "Number of fixed period exclusions for 'Duration of fixed exclusions' in England for 2006/07",
    });
  });
});
