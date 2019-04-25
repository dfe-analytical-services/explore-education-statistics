import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  DataTableResult,
} from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import PageTitle from '@frontend/components/PageTitle';
import PublicationMenu, {
  MenuChangeEventHandler,
} from '@frontend/modules/table-tool/components/PublicationMenu';
import PublicationSubjectMenu from '@frontend/modules/table-tool/components/PublicationSubjectMenu';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import FiltersForm, {
  FilterFormSubmitHandler,
} from '@frontend/prototypes/table-tool/components/FiltersForm';
import LocationFiltersForm from '@frontend/prototypes/table-tool/components/LocationFiltersForm';
import initialMetaSpecification, {
  FilterOption,
  IndicatorOption,
  MetaSpecification,
} from '@frontend/prototypes/table-tool/components/meta/initialSpec';
import publicationSubjectSpec from '@frontend/prototypes/table-tool/components/meta/publicationSubjectSpec';
import TimePeriodDataTable from '@frontend/prototypes/table-tool/components/TimePeriodDataTable';
import TimePeriodFiltersForm from '@frontend/prototypes/table-tool/components/TimePeriodFiltersForm';
import mapOptionValues from '@frontend/prototypes/table-tool/components/utils/mapOptionValues';
import mapValues from 'lodash/mapValues';
import React, { Component, createRef } from 'react';

const defaultPublicationOptions = [
  {
    id: 'early-years-and-schools',
    name: 'Early years and schools',
    topics: [
      {
        id: 'absence-and-exclusions',
        name: 'Absence and exclusions',
        publications: [
          {
            id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
            name: 'Pupil absence',
          },
          {
            id: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
            name: 'Exclusions',
          },
        ],
      },
      {
        id: 'school-and-pupil-numbers',
        name: 'School and pupil numbers',
        publications: [
          {
            id: 'a91d9e05-be82-474c-85ae-4913158406d0',
            name: 'Schools, pupils and their characteristics',
          },
        ],
      },
    ],
  },
];

interface State {
  timePeriods: TimePeriod[];
  locations: {
    [key: string]: string[];
  };
  filters: {
    [key: string]: FilterOption[];
  };
  indicators: IndicatorOption[];
  metaSpecification: MetaSpecification;
  publicationId: string;
  publicationName: string;
  publicationSubjectName: string;
  tableData: DataTableResult[];
}

class PrototypeTableToolPage extends Component<{}, State> {
  public state: State = {
    filters: {},
    timePeriods: [],
    // eslint-disable-next-line react/no-unused-state
    locations: {},
    indicators: [],
    metaSpecification: initialMetaSpecification,
    publicationId: '',
    publicationName: '',
    publicationSubjectName: '',
    tableData: [],
  };

  private filtersRef = createRef<HTMLElement>();

  private dataTableRef = createRef<HTMLElement>();

  private locationFiltersRef = createRef<HTMLElement>();

  private handleMenuChange: MenuChangeEventHandler = async ({
    publicationId,
    publicationName,
  }) => {
    if (!publicationId) {
      return;
    }

    const publicationMeta = await tableBuilderService.getCharacteristicsMeta(
      publicationId,
    );

    const { metaSpecification } = this.state;

    this.setState(
      {
        publicationId,
        publicationName,
        metaSpecification: {
          ...metaSpecification,
          filters: {
            ...metaSpecification.filters,
            characteristics: {
              ...metaSpecification.filters.characteristics,
              options: Object.entries(publicationMeta.characteristics).reduce(
                (acc, [groupKey, group]) => {
                  return {
                    ...acc,
                    [groupKey]: {
                      label: groupKey,
                      options: group.map(option => ({
                        label: option.label,
                        value: option.name,
                      })),
                    },
                  };
                },
                {},
              ),
            },
          },
          indicators: {
            ...Object.entries(publicationMeta.indicators).reduce(
              (acc, [groupKey, group]) => {
                return {
                  ...acc,
                  [groupKey]: {
                    label: groupKey,
                    options: group.map(option => ({
                      label: option.label,
                      unit: option.unit,
                      value: option.name,
                    })),
                  },
                };
              },
              {},
            ),
          },
        },
        publicationSubjectName: '',
        tableData: [],
      },
      // async () => {
      //   this.handleFilterFormSubmit({
      //     categoricalFilters: {
      //       characteristics: [
      //         'Ethnicity_Major_Black_Total',
      //         'Ethnicity_Major_White_Total',
      //         'Ethnicity_Major_Chinese',
      //         'Ethnicity_Major_Mixed_Total',
      //       ],
      //       schoolTypes: [
      //         'StateFundedPrimary',
      //         'StateFundedSecondary',
      //         'Special',
      //       ],
      //     },
      //     indicators: [
      //       'sess_authorised',
      //       'sess_authorised_percent',
      //       'sess_unauthorised',
      //       'sess_unauthorised_percent',
      //     ],
      //     location: {
      //       level: '',
      //       localAuthority: '',
      //       national: '',
      //       region: '',
      //     },
      //     timePeriod: {
      //       start: new TimePeriod(2012, 'AY'),
      //       end: new TimePeriod(2016, 'AY')
      //     },
      //   });
      // }
    );
  };

  private handleFilterFormSubmit: FilterFormSubmitHandler = async ({
    filters,
    indicators,
  }) => {
    const { characteristics, schoolTypes } = filters;

    const { timePeriods } = this.state;

    const start = timePeriods[0].year;
    const end = timePeriods[timePeriods.length - 1].year;

    // TODO: Remove this when timePeriod API finalised
    const formatToAcademicYear = (year: number) => {
      const nextYear = year + 1;
      return parseInt(`${year}${`${nextYear}`.substring(2, 4)}`, 0);
    };

    const { metaSpecification, publicationId } = this.state;

    const { result } = await tableBuilderService.getNationalCharacteristicsData(
      {
        characteristics,
        indicators,
        schoolTypes,
        publicationId,
        startYear: formatToAcademicYear(start),
        endYear: formatToAcademicYear(end),
      },
    );

    const categoricalFiltersByValue = mapValues(
      metaSpecification.filters,
      value => mapOptionValues(value.options),
    );

    const indicatorsByValue = mapOptionValues<IndicatorOption>(
      metaSpecification.indicators,
    );

    this.setState(
      {
        filters: mapValuesWithKeys(filters, ([filterGroup, selectedFilters]) =>
          selectedFilters.map(
            filter => categoricalFiltersByValue[filterGroup][filter],
          ),
        ),
        indicators: indicators.map(indicator => indicatorsByValue[indicator]),
        tableData: result,
      },
      () => {
        if (this.dataTableRef.current) {
          this.dataTableRef.current.scrollIntoView({
            behavior: 'smooth',
            block: 'start',
          });
        }
      },
    );
  };

  public render() {
    const {
      filters,
      indicators,
      timePeriods,
      metaSpecification,
      publicationId,
      publicationName,
      publicationSubjectName,
      tableData,
    } = this.state;

    return (
      <PrototypePage
        breadcrumbs={[{ text: 'Create your own tables online' }]}
        wide
      >
        <PageTitle caption="Table tool" title="Create your own tables online" />

        <p>
          Choose the data and area of interest you want to explore and then use
          filters to create your table.
        </p>

        <p>
          Once you've created your table, you can download the data it contains
          for your own offline analysis.
        </p>

        <section className="govuk-grid-row govuk-form-group">
          <div className="govuk-grid-column-one-third-from-desktop">
            <h2>
              1. Choose your data
              <span className="govuk-hint">Select a data set</span>
            </h2>

            <PublicationMenu
              onChange={this.handleMenuChange}
              options={defaultPublicationOptions}
              value={publicationId}
            />
          </div>
          {publicationId && (
            <div className="govuk-grid-column-two-thirds-from-desktop">
              <>
                <h2>
                  2. Choose your area of interest
                  <span className="govuk-hint">Select an area of interest</span>
                </h2>

                <PublicationSubjectMenu
                  onChange={event =>
                    this.setState(
                      {
                        publicationSubjectName: event.target.value,
                      },
                      () => {
                        if (this.locationFiltersRef.current) {
                          this.locationFiltersRef.current.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start',
                          });
                        }
                      },
                    )
                  }
                  options={publicationSubjectSpec.subjects}
                  value={publicationSubjectName}
                />
              </>
            </div>
          )}
        </section>

        {publicationSubjectName && (
          <section
            className="govuk-grid-row govuk-form-group"
            ref={this.locationFiltersRef}
          >
            <div className="govuk-grid-column-one-third-from-desktop">
              <h2>
                3. Choose location filters
                <span className="govuk-hint">
                  Select at least one location to compare.
                </span>
              </h2>

              <LocationFiltersForm
                specification={metaSpecification}
                onSubmit={values => {
                  this.setState(
                    {
                      // eslint-disable-next-line react/no-unused-state
                      locations: {
                        ...values,
                      },
                    },
                    () => {
                      if (this.filtersRef.current) {
                        this.filtersRef.current.scrollIntoView({
                          behavior: 'smooth',
                          block: 'start',
                        });
                      }
                    },
                  );
                }}
              />
            </div>
          </section>
        )}

        {publicationSubjectName && (
          <section
            className="govuk-grid-row govuk-form-group"
            ref={this.locationFiltersRef}
          >
            <div className="govuk-grid-column-one-third-from-desktop">
              <h2>
                4. Choose time period
                <span className="govuk-hint">Select a start and end date.</span>
              </h2>

              <TimePeriodFiltersForm
                specification={metaSpecification}
                onSubmit={values => {
                  this.setState(
                    {
                      timePeriods: TimePeriod.createRange(
                        TimePeriod.fromString(values.start),
                        TimePeriod.fromString(values.end),
                      ),
                    },
                    () => {
                      if (this.filtersRef.current) {
                        this.filtersRef.current.scrollIntoView({
                          behavior: 'smooth',
                          block: 'start',
                        });
                      }
                    },
                  );
                }}
              />
            </div>
          </section>
        )}

        {publicationSubjectName && (
          <section className="govuk-form-group" ref={this.filtersRef}>
            <h2>
              5. Create your table for '{publicationName}'
              <span className="govuk-hint">
                Select at least 1 option from under each of the following
                headings
              </span>
            </h2>

            <FiltersForm
              onSubmit={this.handleFilterFormSubmit}
              specification={metaSpecification}
            />
          </section>
        )}

        {tableData.length > 0 && (
          <section ref={this.dataTableRef}>
            <h2>5. Explore data for '{publicationName}'</h2>

            <TimePeriodDataTable
              filters={filters}
              indicators={indicators}
              timePeriods={timePeriods}
              results={tableData}
            />

            <ul className="govuk-list">
              <li>
                <a href="#download">Download data (.csv)</a>
              </li>
              <li>
                <a href="#api">Access developer API</a>
              </li>
              <li>
                <a href="#methodology">Methodology</a>
              </li>
              <li>
                <a href="#contact">Contact</a>
              </li>
            </ul>
          </section>
        )}
      </PrototypePage>
    );
  }
}

export default PrototypeTableToolPage;
