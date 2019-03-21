import range from 'lodash/range';
import React, { Component, createRef } from 'react';
import PageTitle from 'src/components/PageTitle';
import Tabs from 'src/components/Tabs';
import TabsSection from 'src/components/TabsSection';
import CharacteristicsDataTable from 'src/modules/table-tool/components/CharacteristicsDataTable';
import CharacteristicsFilterForm, {
  CharacteristicsFilterFormSubmitHandler,
} from 'src/modules/table-tool/components/CharacteristicsFilterForm';
import PublicationMenu, {
  MenuChangeEventHandler,
} from 'src/modules/table-tool/components/PublicationMenu';
import PublicationSubjectMenu from 'src/modules/table-tool/components/PublicationSubjectMenu';
import PrototypePage from 'src/prototypes/components/PrototypePage';
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
    characteristics: string[];
    indicators: string[];
    schoolTypes: SchoolType[];
    years: number[];
  };
  publicationId: string;
  publicationMeta: Pick<PublicationMeta, 'characteristics' | 'indicators'>;
  publicationName: string;
  publicationSubjectId: string;
  tableData: DataTableResult[];
}

class PrototypeTableToolPage extends Component<{}, State> {
  private readonly defaultFilters: State['filters'] = {
    characteristics: [],
    indicators: [],
    schoolTypes: [],
    years: [],
  };

  public state: State = {
    filters: {
      ...this.defaultFilters,
    },
    publicationId: '',
    publicationMeta: {
      characteristics: {},
      indicators: {},
    },
    publicationName: '',
    publicationSubjectId: '',
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
      publicationSubjectId: '',
      tableData: [],
    });
  };

  private handleFilterFormSubmit: CharacteristicsFilterFormSubmitHandler = async ({
    characteristics,
    indicators,
    schoolTypes,
    startYear,
    endYear,
  }) => {
    const formatToAcademicYear = (year: number) =>
      parseInt(`${year}${`${year + 1}`.substring(2, 4)}`, 0);

    const { result } = await tableBuilderService.getNationalCharacteristicsData(
      {
        characteristics,
        indicators,
        schoolTypes,
        endYear: formatToAcademicYear(endYear),
        publicationId: this.state.publicationId,
        startYear: formatToAcademicYear(startYear),
      },
    );

    const years = range(startYear, endYear + 1).map(formatToAcademicYear);

    this.setState(
      {
        filters: {
          characteristics,
          indicators,
          schoolTypes,
          years,
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
      publicationMeta,
      publicationId,
      publicationName,
      publicationSubjectId,
      tableData,
    } = this.state;

    return (
      <PrototypePage breadcrumbs={[{ text: 'Create your own table' }]}>
        <PageTitle caption="Table tool" title="Create your own table" />

        <ul>
          <li>You can create your own tables from our publication data.</li>
          <li>
            You can filter it by things like year, school type, area or pupil
            characteristics.
          </li>
          <li>
            You can also download it, visualise it or copy and paste it as you
            need.
          </li>
        </ul>

        <section className="govuk-grid-row">
          <div className="govuk-grid-column-one-half">
            <h2>
              1. Choose a publication
              <span className="govuk-hint">
                Pick a publication below to create a table for it
              </span>
            </h2>

            <PublicationMenu
              onChange={this.handleMenuChange}
              options={defaultPublicationOptions}
              value={publicationId}
            />
          </div>
          <div className="govuk-grid-column-one-half">
            {publicationId && (
              <>
                <h2>
                  2. Choose a subject
                  <span className="govuk-hint">
                    Pick a subject from '{publicationName}'
                  </span>
                </h2>

                <PublicationSubjectMenu
                  onChange={event =>
                    this.setState(
                      {
                        publicationSubjectId: event.target.value,
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
                  options={[
                    {
                      id: 'natcharacteristics',
                      name: 'National characteristics',
                    },
                    {
                      id: 'lacharacteristics',
                      name: 'Local authority characteristics',
                    },
                    {
                      id: 'geoglevels',
                      name: 'Geographic levels',
                    },
                  ]}
                  value={publicationSubjectId}
                />
              </>
            )}
          </div>
        </section>

        {publicationSubjectId && (
          <section className="govuk-grid-row" ref={this.filtersRef}>
            <div className="govuk-grid-column-full">
              <h2>
                3. Filter statistics from '{publicationName}'
                <span className="govuk-hint">
                  Select any options you are interested in from the groups
                  below.
                </span>
              </h2>

              <Tabs>
                <TabsSection id="characteristics" title="Characteristics">
                  <CharacteristicsFilterForm
                    publicationMeta={publicationMeta}
                    onSubmit={this.handleFilterFormSubmit}
                  />
                </TabsSection>
              </Tabs>
            </div>
          </section>
        )}

        {tableData.length > 0 && (
          <section ref={this.dataTableRef}>
            <h2>3. Explore statistics from '{publicationName}'</h2>

            <CharacteristicsDataTable
              characteristics={filters.characteristics}
              characteristicsMeta={publicationMeta.characteristics}
              indicators={filters.indicators}
              indicatorsMeta={publicationMeta.indicators}
              results={tableData}
              schoolTypes={filters.schoolTypes}
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
