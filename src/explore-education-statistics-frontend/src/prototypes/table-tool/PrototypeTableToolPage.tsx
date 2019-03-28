import range from 'lodash/range';
import React, { Component, createRef } from 'react';
import PageTitle from 'src/components/PageTitle';
import PublicationMenu, {
  MenuChangeEventHandler,
} from 'src/modules/table-tool/components/PublicationMenu';
import PublicationSubjectMenu from 'src/modules/table-tool/components/PublicationSubjectMenu';
import PrototypePage from 'src/prototypes/components/PrototypePage';
import DataTable from 'src/prototypes/table-tool/components/DataTable';
import FiltersForm, {
  FilterFormSubmitHandler,
} from 'src/prototypes/table-tool/components/FiltersForm';
import initialMetaSpecification, {
  MetaSpecification,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import publicationSubjectSpec from 'src/prototypes/table-tool/components/meta/publicationSubjectSpec';
import tableBuilderService, {
  DataTableResult,
  PublicationMeta,
} from 'src/services/tableBuilderService';
import SchoolType from 'src/services/types/SchoolType';

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
    indicators: string[];
    categorical: {
      [key: string]: string[];
    };
    years: number[];
  };
  metaSpecification: MetaSpecification;
  publicationId: string;
  publicationMeta: Pick<PublicationMeta, 'characteristics' | 'indicators'>;
  publicationName: string;
  publicationSubjectName: string;
  tableData: DataTableResult[];
}

class PrototypeTableToolPage extends Component<{}, State> {
  private readonly defaultFilters: State['filters'] = {
    categorical: {},
    indicators: [],
    years: [],
  };

  public state: State = {
    filters: {
      ...this.defaultFilters,
    },
    metaSpecification: initialMetaSpecification,
    publicationId: '',
    publicationMeta: {
      characteristics: {},
      indicators: {},
    },
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

    this.setState({
      publicationId,
      publicationMeta,
      publicationName,
      filters: {
        ...this.defaultFilters,
      },
      metaSpecification: {
        ...this.state.metaSpecification,
        categoricalFilters: {
          ...this.state.metaSpecification.categoricalFilters,
          characteristics: {
            ...this.state.metaSpecification.categoricalFilters.characteristics,
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
    });
  };

  private handleFilterFormSubmit: FilterFormSubmitHandler = async ({
    categoricalFilters,
    indicators,
    startDate,
    endDate,
  }) => {
    const { characteristics } = categoricalFilters;
    const schoolTypes = categoricalFilters.schoolTypes as SchoolType[];

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

    const years = range(startDate, endDate + 1).map(formatToAcademicYear);

    this.setState(
      {
        filters: {
          indicators,
          years,
          categorical: categoricalFilters,
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
      publicationMeta,
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
          <div className="govuk-grid-column-one-third-from-desktop">
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

        {publicationSubjectName && (
          <section className="govuk-form-group" ref={this.filtersRef}>
            <h2>
              3. Choose your filters for '{publicationName}'
              <span className="govuk-hint">
                Select any combination of filters.
              </span>
            </h2>

            <FiltersForm
              specification={metaSpecification}
              onSubmit={this.handleFilterFormSubmit}
            />
          </section>
        )}

        {tableData.length > 0 && (
          <section ref={this.dataTableRef}>
            <h2>4. Explore data for '{publicationName}'</h2>

            <DataTable
              characteristics={filters.categorical.characteristics}
              characteristicsMeta={publicationMeta.characteristics}
              indicators={filters.indicators}
              indicatorsMeta={publicationMeta.indicators}
              results={tableData}
              schoolTypes={filters.categorical.schoolTypes}
              years={filters.years}
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
