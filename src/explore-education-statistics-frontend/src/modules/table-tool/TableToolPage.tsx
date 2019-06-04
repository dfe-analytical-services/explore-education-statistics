import ModalConfirm from '@common/components/ModalConfirm';
import { ConfirmContextProvider } from '@common/context/ConfirmContext';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption,
  IndicatorOption,
  PublicationSubject,
  PublicationSubjectMeta,
  TableData,
  ThemeMeta,
} from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types/util';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import mapValues from 'lodash/mapValues';
import React, { Component } from 'react';
import FiltersForm, { FilterFormSubmitHandler } from './components/FiltersForm';
import LocationFiltersForm, {
  LocationFiltersFormSubmitHandler,
} from './components/LocationFiltersForm';
import PublicationForm, {
  PublicationFormSubmitHandler,
} from './components/PublicationForm';
import PublicationSubjectForm, {
  PublicationSubjectFormSubmitHandler,
} from './components/PublicationSubjectForm';
import TimePeriodDataTable from './components/TimePeriodDataTable';
import TimePeriodForm from './components/TimePeriodForm';
import mapOptionValues from './components/utils/mapOptionValues';
import Wizard from './components/Wizard';
import WizardStep from './components/WizardStep';
import WizardStepHeading from './components/WizardStepHeading';

export interface PublicationOptions {
  id: string;
  title: string;
  topics: {
    id: string;
    title: string;
    publications: {
      id: string;
      title: string;
      slug: string;
    }[];
  }[];
}

interface Props {
  themeMeta: ThemeMeta[];
}

interface State {
  timePeriods: TimePeriod[];
  locations: Dictionary<FilterOption[]>;
  filters: Dictionary<FilterOption[]>;
  indicators: IndicatorOption[];
  publication?: PublicationOptions['topics'][0]['publications'][0];
  subjects: PublicationSubject[];
  subjectId: string;
  subjectName: string;
  subjectMeta: PublicationSubjectMeta;
  tableData: TableData['result'];
}

class TableToolPage extends Component<Props, State> {
  public state: State = {
    filters: {},
    timePeriods: [],
    locations: {},
    indicators: [],
    subjectName: '',
    subjectId: '',
    subjectMeta: {
      timePeriod: {
        hint: '',
        legend: '',
        options: [],
      },
      locations: {},
      indicators: {},
      filters: {},
    },
    subjects: [],
    tableData: [],
  };

  public static async getInitialProps() {
    const themeMeta = await tableBuilderService.getThemes();
    return { themeMeta };
  }

  private handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publicationId,
  }) => {
    const { themeMeta } = this.props;
    const publication = themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.id === publicationId);

    if (!publication) {
      return;
    }

    const { subjects } = await tableBuilderService.getPublicationMeta(
      publicationId,
    );

    this.setState({
      publication,
      subjects,
      subjectName: '',
      tableData: [],
    });
  };

  private handlePublicationSubjectFormSubmit: PublicationSubjectFormSubmitHandler = async ({
    subjectId,
    subjectName,
  }) => {
    const subjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      subjectId,
    );

    this.setState({
      subjectMeta,
      subjectName,
      subjectId,
    });
  };

  private handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = values => {
    const { subjectMeta } = this.state;

    this.setState({
      locations: mapValuesWithKeys(
        values.locations,
        ([locationLevel, locations]) =>
          locations.map(
            location =>
              subjectMeta.locations[locationLevel].options.find(
                option => option.value === location,
              ) as FilterOption,
          ),
      ),
    });
  };

  private handleFiltersFormSubmit: FilterFormSubmitHandler = async ({
    filters,
    indicators,
  }) => {
    const { subjectId, timePeriods, subjectMeta } = this.state;

    const { result } = await tableBuilderService.getTableData({
      subjectId,
      filters: Object.values(filters).flat(),
      indicators,
      startYear: timePeriods[0].year,
      endYear: timePeriods[timePeriods.length - 1].year,
      geographicLevel: 'National',
    });

    const filtersByValue = mapValues(subjectMeta.filters, value =>
      mapOptionValues(value.options),
    );

    const indicatorsByValue = mapOptionValues<IndicatorOption>(
      subjectMeta.indicators,
    );

    this.setState({
      filters: mapValuesWithKeys(filters, ([filterGroup, selectedFilters]) =>
        selectedFilters.map(filter => filtersByValue[filterGroup][filter]),
      ),
      indicators: indicators.map(indicator => indicatorsByValue[indicator]),
      tableData: result,
    });
  };

  public render() {
    const { themeMeta } = this.props;
    const {
      filters,
      indicators,
      locations,
      timePeriods,
      publication,
      subjectName,
      subjectMeta,
      subjects,
      tableData,
    } = this.state;

    return (
      <Page breadcrumbs={[{ name: 'Create your own tables online' }]} wide>
        <PageTitle caption="Table tool" title="Create your own tables online" />

        <p>
          Choose the data and area of interest you want to explore and then use
          filters to create your table.
        </p>

        <p>
          Once you've created your table, you can download the data it contains
          for your own offline analysis.
        </p>

        <ConfirmContextProvider>
          <Wizard id="tableTool-steps">
            <WizardStep>
              {stepProps => (
                <PublicationForm
                  {...stepProps}
                  options={themeMeta}
                  onSubmit={this.handlePublicationFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <PublicationSubjectForm
                  {...stepProps}
                  options={subjects}
                  onSubmit={this.handlePublicationSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  options={subjectMeta.locations}
                  onSubmit={this.handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  options={subjectMeta.timePeriod.options}
                  onSubmit={values => {
                    this.setState({
                      timePeriods: TimePeriod.createRange(
                        TimePeriod.fromString(values.start),
                        TimePeriod.fromString(values.end),
                      ),
                    });
                  }}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <FiltersForm
                  {...stepProps}
                  onSubmit={this.handleFiltersFormSubmit}
                  specification={subjectMeta}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <>
                  <WizardStepHeading {...stepProps}>
                    Explore data
                  </WizardStepHeading>

                  {tableData.length > 0 && (
                    <>
                      <div className="govuk-!-margin-bottom-2">
                        <TimePeriodDataTable
                          filters={filters}
                          indicators={indicators}
                          publicationName={publication ? publication.title : ''}
                          subjectName={subjectName}
                          locations={locations}
                          timePeriods={timePeriods}
                          results={tableData}
                        />
                      </div>

                      <ul className="govuk-list">
                        {publication && (
                          <li>
                            <a href={publication.slug}>Go to publication</a>
                          </li>
                        )}
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
                    </>
                  )}
                </>
              )}
            </WizardStep>
          </Wizard>

          <ModalConfirm title="Go back to previous step">
            <p>You will lose any changes you have made in the current step.</p>
          </ModalConfirm>
        </ConfirmContextProvider>
      </Page>
    );
  }
}

export default TableToolPage;
