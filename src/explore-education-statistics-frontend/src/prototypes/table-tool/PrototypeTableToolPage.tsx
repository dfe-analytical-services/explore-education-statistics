import range from 'lodash/range';
import React, { Component, createRef } from 'react';
import PageTitle from 'src/components/PageTitle';
import PublicationMenu, {
  MenuChangeEventHandler,
} from 'src/modules/table-tool/components/PublicationMenu';
import PublicationSubjectMenu from 'src/modules/table-tool/components/PublicationSubjectMenu';
import PrototypePage from 'src/prototypes/components/PrototypePage';
import { FilterFormSubmitHandler } from 'src/prototypes/table-tool/components/FiltersForm';
import initialMetaSpecification, {
  FilterOption,
  IndicatorOption,
  MetaSpecification,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import publicationSubjectSpec from 'src/prototypes/table-tool/components/meta/publicationSubjectSpec';
import TimePeriodDataTable from 'src/prototypes/table-tool/components/TimePeriodDataTable';
import mapOptionValues from 'src/prototypes/table-tool/components/utils/mapOptionValues';
import tableBuilderService, {
  DataTableResult,
} from 'src/services/tableBuilderService';

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
  filters: {
    indicators: IndicatorOption[];
    categorical: {
      [key: string]: FilterOption[];
    };
    years: number[];
  };
  metaSpecification: MetaSpecification;
  publicationId: string;
  publicationName: string;
  publicationSubjectName: string;
  tableData: DataTableResult[];
}

class PrototypeTableToolPage extends Component<{}, State> {
  private readonly defaultFilters: State['filters'] = {
    categorical: {
      characteristics: [],
      schoolTypes: [],
    },
    indicators: [],
    years: [],
  };

  public state: State = {
    filters: {
      ...this.defaultFilters,
    },
    metaSpecification: initialMetaSpecification,
    publicationId: '',
    publicationName: '',
    publicationSubjectName: '',
    tableData: [],
  };

  private filtersRef = createRef<HTMLElement>();
  private dataTableRef = createRef<HTMLElement>();

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

    this.setState(
      {
        publicationId,
        publicationName,
        filters: {
          ...this.defaultFilters,
        },
        metaSpecification: {
          ...this.state.metaSpecification,
          categoricalFilters: {
            ...this.state.metaSpecification.categoricalFilters,
            characteristics: {
              ...this.state.metaSpecification.categoricalFilters
                .characteristics,
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
      async () => {
        const characteristics = [
          'Ethnicity_Major_Black_Total',
          'Ethnicity_Major_White_Total',
          'Ethnicity_Major_Chinese',
          'Ethnicity_Major_Mixed_Total',
        ];

        const schoolTypes = [
          'State_Funded_Primary',
          'State_Funded_Secondary',
          'Special',
        ];

        const indicators = [
          'sess_authorised',
          'sess_authorised_percent',
          'sess_unauthorised',
          'sess_unauthorised_percent',
        ];

        const formatToAcademicYear = (year: string | number) => {
          const nextYear = parseInt(year as string, 0) + 1;
          return parseInt(`${year}${`${nextYear}`.substring(2, 4)}`, 0);
        };

        const startDate = 2012;
        const endDate = 2016;

        const {
          result,
        } = await tableBuilderService.getNationalCharacteristicsData({
          characteristics,
          indicators,
          schoolTypes,
          endYear: formatToAcademicYear(endDate),
          publicationId: this.state.publicationId,
          startYear: formatToAcademicYear(startDate),
        });

        const schoolTypesByValue = mapOptionValues(
          this.state.metaSpecification.categoricalFilters.schoolTypes.options,
        );
        const characteristicsByValue = mapOptionValues(
          this.state.metaSpecification.categoricalFilters.characteristics
            .options,
        );

        const indicatorsByValue = mapOptionValues<IndicatorOption>(
          this.state.metaSpecification.indicators,
        );

        const years = range(startDate, endDate + 1).map(formatToAcademicYear);

        this.setState(
          {
            filters: {
              years,
              categorical: {
                characteristics: characteristics.map(
                  characteristic => characteristicsByValue[characteristic],
                ),
                schoolTypes: schoolTypes.map(
                  schoolType => schoolTypesByValue[schoolType],
                ),
              },
              indicators: indicators.map(
                indicator => indicatorsByValue[indicator],
              ),
            },
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
      },
    );
  };

  private handleFilterFormSubmit: FilterFormSubmitHandler = async ({
    categoricalFilters,
    indicators,
    startDate,
    endDate,
  }) => {
    const { characteristics, schoolTypes } = categoricalFilters;

    const formatToAcademicYear = (year: string | number) => {
      const nextYear = parseInt(year as string, 0) + 1;
      return parseInt(`${year}${`${nextYear}`.substring(2, 4)}`, 0);
    };

    const { result } = await tableBuilderService.getNationalCharacteristicsData(
      {
        characteristics,
        indicators,
        schoolTypes,
        endYear: formatToAcademicYear(endDate),
        publicationId: this.state.publicationId,
        startYear: formatToAcademicYear(startDate),
      },
    );

    const schoolTypesByValue = mapOptionValues(
      this.state.metaSpecification.categoricalFilters.schoolTypes.options,
    );
    const characteristicsByValue = mapOptionValues(
      this.state.metaSpecification.categoricalFilters.characteristics.options,
    );

    const indicatorsByValue = mapOptionValues<IndicatorOption>(
      this.state.metaSpecification.indicators,
    );

    const years = range(startDate, endDate + 1).map(formatToAcademicYear);

    this.setState(
      {
        filters: {
          years,
          categorical: {
            characteristics: characteristics.map(
              characteristic => characteristicsByValue[characteristic],
            ),
            schoolTypes: schoolTypes.map(
              schoolType => schoolTypesByValue[schoolType],
            ),
          },
          indicators: indicators.map(indicator => indicatorsByValue[indicator]),
        },
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
          the filters to create your table.
        </p>

        <p>
          Once you've built your table, you can download the data it contains
          for your own offline analysis.
        </p>

        <section className="govuk-grid-row govuk-form-group">
          <div className="govuk-grid-column-one-third-from-desktop">
            <h2>
              1. Choose your data
              <span className="govuk-hint">Select a data set.</span>
            </h2>

            <PublicationMenu
              onChange={this.handleMenuChange}
              options={defaultPublicationOptions}
              value={publicationId}
            />
          </div>
          <div className="govuk-grid-column-two-thirds-from-desktop">
            {publicationId && (
              <>
                <h2>
                  2. Choose your area of interest
                  <span className="govuk-hint">
                    Select an area of interest.
                  </span>
                </h2>

                <PublicationSubjectMenu
                  onChange={event =>
                    this.setState(
                      {
                        publicationSubjectName: event.target.value,
                      },
                      () => {
                        if (this.filtersRef.current) {
                          this.filtersRef.current.scrollIntoView({
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
            )}
          </div>
        </section>

        {/*{publicationSubjectName && (*/}
        {/*<section className="govuk-form-group" ref={this.filtersRef}>*/}
        {/*<h2>*/}
        {/*3. Choose your filters for '{publicationName}'*/}
        {/*<span className="govuk-hint">*/}
        {/*Select any combination of filters.*/}
        {/*</span>*/}
        {/*</h2>*/}

        {/*<FiltersForm*/}
        {/*onSubmit={this.handleFilterFormSubmit}*/}
        {/*specification={metaSpecification}*/}
        {/*/>*/}
        {/*</section>*/}
        {/*)}*/}

        {tableData.length > 0 && (
          <section ref={this.dataTableRef}>
            <h2>4. Explore data for '{publicationName}'</h2>

            <TimePeriodDataTable filters={filters} results={tableData} />

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
