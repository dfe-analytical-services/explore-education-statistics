import range from 'lodash/range';
import React, { Component, createRef } from 'react';
import Page from 'src/components/Page';
import PageTitle from 'src/components/PageTitle';
import Tabs from 'src/components/Tabs';
import TabsSection from 'src/components/TabsSection';
import {
  DataTableResult,
  getCharacteristicsMeta,
  getNationalCharacteristicsData,
  PublicationMeta,
  SchoolType,
} from '../../services/tableBuilderService';
import CharacteristicsDataTable from './components/CharacteristicsDataTable';
import CharacteristicsFilterForm, {
  CharacteristicsFilterFormSubmitHandler,
} from './components/CharacteristicsFilterForm';
import PublicationMenu, {
  MenuChangeEventHandler,
} from './components/PublicationMenu';

interface State {
  filters: {
    attributes: string[];
    characteristics: string[];
    schoolTypes: SchoolType[];
    years: number[];
  };
  publicationId: string;
  publicationMeta: Pick<PublicationMeta, 'attributes' | 'characteristics'>;
  publicationName: string;
  tableData: DataTableResult[];
}

class TableToolPage extends Component<{}, State> {
  public state: State = {
    filters: {
      attributes: [],
      characteristics: [],
      schoolTypes: [],
      years: [],
    },
    publicationId: '',
    publicationMeta: {
      attributes: {},
      characteristics: {},
    },
    publicationName: '',
    tableData: [],
  };

  private filtersRef = createRef<HTMLDivElement>();
  private dataTableRef = createRef<HTMLDivElement>();

  private handleMenuChange: MenuChangeEventHandler = async menuOption => {
    if (!menuOption) {
      return;
    }

    const menuOptions = {
      EXCLUSIONS: {
        id: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
        label: 'Exclusions',
      },
      PUPIL_ABSENCE: {
        id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
        label: 'Pupil absence',
      },
      SCHOOLS_PUPILS_CHARACTERISTICS: {
        id: 'a91d9e05-be82-474c-85ae-4913158406d0',
        label: 'Schools, pupils and their characteristics',
      },
    };

    const publication = menuOptions[menuOption];

    const { data } = await getCharacteristicsMeta(publication.id);

    this.setState(
      {
        publicationId: publication.id,
        publicationMeta: data,
        publicationName: publication.label,
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
  };

  private handleFilterFormSubmit: CharacteristicsFilterFormSubmitHandler = async ({
    attributes,
    characteristics,
    schoolTypes,
    startYear,
    endYear,
  }) => {
    const formatToAcademicYear = (year: number) =>
      parseInt(`${year}${`${year + 1}`.substring(2, 4)}`, 0);

    const { data } = await getNationalCharacteristicsData(
      this.state.publicationId,
      attributes,
      characteristics,
      schoolTypes,
      formatToAcademicYear(startYear),
      formatToAcademicYear(endYear),
    );

    const years = range(startYear, endYear + 1).map(formatToAcademicYear);

    this.setState(
      {
        filters: {
          attributes,
          characteristics,
          schoolTypes,
          years,
        },
        tableData: data.result,
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
    const { filters, publicationMeta, publicationName, tableData } = this.state;

    return (
      <Page breadcrumbs={[{ name: 'Explore statistics' }]}>
        <PageTitle caption="National level" title="Explore statistics" />

        <ul>
          <li>
            You can explore all the DfE statistics available at national level
            here.
          </li>
          <li>
            Once you've chosen your data you can view it by year, school type,
            area or pupil characteristics.
          </li>
          <li>
            You can also download it, visualise it or copy and paste it as you
            need.
          </li>
        </ul>

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-full">
            <PublicationMenu onChange={this.handleMenuChange} />
          </div>
        </div>

        {publicationName && (
          <div className="govuk-grid-row" ref={this.filtersRef}>
            <div className="govuk-grid-column-full">
              <h2>
                2. Filter statistics from '{publicationName}'
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
          </div>
        )}

        {tableData.length > 0 && (
          <div ref={this.dataTableRef}>
            <h2>3. Explore statistics from '{publicationName}'</h2>

            <CharacteristicsDataTable
              attributes={filters.attributes}
              attributesMeta={publicationMeta.attributes}
              characteristics={filters.characteristics}
              characteristicsMeta={publicationMeta.characteristics}
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
          </div>
        )}
      </Page>
    );
  }
}

export default TableToolPage;
