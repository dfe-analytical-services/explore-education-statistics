import { ConfirmContextProvider } from '@common/context/ConfirmContext';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption,
  IndicatorOption,
  PublicationSubject,
  PublicationSubjectMeta,
  TableData,
  TableDataQuery,
  ThemeMeta,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import ButtonText from '@common/components/ButtonText';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PreviousStepModalConfirm from '@frontend/modules/table-tool/components/PreviousStepModalConfirm';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@frontend/modules/table-tool/components/types/filters';
import TimePeriod, {
  parseYearCodeTuple,
} from '@frontend/modules/table-tool/components/types/TimePeriod';
import mapValues from 'lodash/mapValues';
import { NextContext } from 'next';
import Router from 'next/router';
import React, { Component, MouseEventHandler, createRef } from 'react';
import getDefaultTableHeaderConfig from '@frontend/modules/table-tool/utils/tableHeaders';
import DownloadCsvButton from './components/DownloadCsvButton';
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
import TimePeriodForm, {
  TimePeriodFormSubmitHandler,
} from './components/TimePeriodForm';
import mapOptionValues from './components/utils/mapOptionValues';
import Wizard from './components/Wizard';
import WizardStep from './components/WizardStep';
import WizardStepHeading from './components/WizardStepHeading';
import TableHeadersForm, {
  TableHeadersFormValues,
} from './components/TableHeadersForm';

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
  publicationId: string;
}

interface State {
  startYear?: number;
  startCode?: string;
  endYear?: number;
  endCode?: string;
  locations: Dictionary<LocationFilter[]>;
  filters: Dictionary<CategoryFilter[]>;
  indicators: Indicator[];
  publication?: PublicationOptions['topics'][0]['publications'][0];
  subjects: PublicationSubject[];
  subjectId: string;
  subjectName: string;
  subjectMeta: PublicationSubjectMeta;
  timePeriodRange: TimePeriod[];
  tableData: TableData['result'];
  footnotes: TableData['footnotes'];
  tableHeaders: TableHeadersFormValues;
}

class TableToolPage extends Component<Props, State> {
  public state: State = {
    filters: {},
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
    timePeriodRange: [],
    tableData: [],
    footnotes: [],
    tableHeaders: {
      columnGroups: [],
      columns: [],
      rowGroups: [],
      rows: [],
    },
  };

  private dataTableRef = createRef<HTMLTableElement>();

  public static async getInitialProps({ query }: NextContext) {
    const themeMeta = await tableBuilderService.getThemes();

    const publication = themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.slug === query.publicationSlug);

    return {
      themeMeta,
      publicationId: publication ? publication.id : '',
    };
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
      footnotes: [],
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

  private handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations,
  }) => {
    const { subjectId } = this.state;

    const subjectMeta = await tableBuilderService.filterPublicationSubjectMeta({
      ...locations,
      subjectId,
    });

    this.setState(prevState => ({
      subjectMeta: {
        ...prevState.subjectMeta,
        timePeriod: subjectMeta.timePeriod,
      },
      locations: mapValuesWithKeys(
        locations,
        (locationLevel, locationOptions) =>
          locationOptions
            .map(location =>
              prevState.subjectMeta.locations[locationLevel].options.find(
                option => option.value === location,
              ),
            )
            .filter(option => typeof option !== 'undefined')
            .map(
              option =>
                new LocationFilter(option as FilterOption, locationLevel),
            ),
      ),
    }));
  };

  private handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const { subjectId, locations } = this.state;

    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const subjectMeta = await tableBuilderService.filterPublicationSubjectMeta({
      ...mapValues(locations, locationLevel =>
        locationLevel.map(location => location.value),
      ),
      subjectId,
      timePeriod: {
        startYear,
        startCode,
        endYear,
        endCode,
      },
    });

    this.setState(prevState => ({
      startYear,
      startCode,
      endYear,
      endCode,
      subjectMeta: {
        ...prevState.subjectMeta,
        filters: subjectMeta.filters,
      },
    }));
  };

  private createQuery = (
    filters: Dictionary<CategoryFilter[]>,
    indicators: Indicator[],
  ): TableDataQuery => {
    const {
      subjectId,
      startYear,
      startCode,
      endYear,
      endCode,
      locations,
    } = this.state;

    if (!startYear || !startCode || !endYear || !endCode) {
      throw new Error('Missing required timePeriod parameters');
    }

    return {
      ...mapValues(locations, locationLevel =>
        locationLevel.map(location => location.value),
      ),
      subjectId,
      indicators: indicators.map(indicator => indicator.value),
      filters: Object.values(filters).flatMap(categoryFilters =>
        categoryFilters.flatMap(filter => filter.value),
      ),
      timePeriod: {
        startYear,
        startCode,
        endYear,
        endCode,
      },
    };
  };

  private handleFiltersFormSubmit: FilterFormSubmitHandler = async values => {
    const {
      startYear,
      startCode,
      endYear,
      endCode,
      subjectMeta,
      locations,
    } = this.state;

    if (!startYear || !startCode || !endYear || !endCode) {
      return;
    }

    const filtersByValue = mapValues(subjectMeta.filters, value =>
      mapOptionValues(value.options),
    );

    const indicatorsByValue = mapOptionValues<IndicatorOption>(
      subjectMeta.indicators,
    );

    const filters = mapValuesWithKeys(
      values.filters,
      (filterGroup, selectedFilters) =>
        selectedFilters.map(
          filter =>
            new CategoryFilter(
              filtersByValue[filterGroup][filter],
              filter === subjectMeta.filters[filterGroup].totalValue,
            ),
        ),
    );

    const indicators = values.indicators.map(
      indicator => new Indicator(indicatorsByValue[indicator]),
    );

    const {
      footnotes,
      timePeriodRange,
      result,
    } = await tableBuilderService.getTableData(
      this.createQuery(filters, indicators),
    );

    const timePeriods = timePeriodRange.map(
      timePeriod => new TimePeriod(timePeriod),
    );

    this.setState({
      filters,
      indicators,
      timePeriodRange: timePeriods,
      tableData: result,
      footnotes,
      tableHeaders: getDefaultTableHeaderConfig(
        indicators,
        filters,
        timePeriods,
        Object.values(locations).flat(),
      ),
    });
  };

  private handlePermalinkClick: MouseEventHandler = async () => {
    const { filters, indicators } = this.state;

    const { url } = await tableBuilderService.getTablePermalink(
      this.createQuery(filters, indicators),
    );

    Router.push(url);
  };

  public render() {
    const { themeMeta, publicationId } = this.props;
    const {
      filters,
      indicators,
      locations,
      publication,
      subjectName,
      subjectMeta,
      subjects,
      timePeriodRange,
      tableData,
      footnotes,
      tableHeaders,
    } = this.state;

    const locationsList = Object.values(locations).flat();

    return (
      <Page title="Create your own tables online" caption="Table Tool" wide>
        <p>
          Choose the data and area of interest you want to explore and then use
          filters to create your table.
        </p>

        <p>
          Once you've created your table, you can download the data it contains
          for your own offline analysis.
        </p>

        <ConfirmContextProvider>
          {({ askConfirm }) => (
            <>
              <Wizard
                id="tableTool-steps"
                onStepChange={async (nextStep, previousStep) => {
                  if (nextStep < previousStep) {
                    const confirmed = await askConfirm();
                    return confirmed ? nextStep : previousStep;
                  }

                  return nextStep;
                }}
              >
                <WizardStep>
                  {stepProps => (
                    <PublicationForm
                      {...stepProps}
                      publicationId={publicationId}
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
                      onSubmit={this.handleTimePeriodFormSubmit}
                    />
                  )}
                </WizardStep>
                <WizardStep>
                  {stepProps => (
                    <FiltersForm
                      {...stepProps}
                      onSubmit={this.handleFiltersFormSubmit}
                      subjectMeta={subjectMeta}
                    />
                  )}
                </WizardStep>
                <WizardStep>
                  {stepProps => (
                    <>
                      <WizardStepHeading {...stepProps}>
                        Explore data
                      </WizardStepHeading>

                      <div className="govuk-!-margin-bottom-4">
                        <TableHeadersForm
                          initialValues={tableHeaders}
                          onSubmit={tableHeaderConfig => {
                            this.setState({ tableHeaders: tableHeaderConfig });

                            if (this.dataTableRef.current) {
                              this.dataTableRef.current.scrollIntoView({
                                behavior: 'smooth',
                                block: 'start',
                              });
                            }
                          }}
                        />
                        <TimePeriodDataTable
                          ref={this.dataTableRef}
                          filters={filters}
                          indicators={indicators}
                          publicationName={publication ? publication.title : ''}
                          subjectName={subjectName}
                          locations={locationsList}
                          timePeriods={timePeriodRange}
                          results={tableData}
                          footnotes={footnotes}
                          tableHeadersConfig={tableHeaders}
                        />
                      </div>

                      {publication && (
                        <>
                          <h3>Additional options</h3>

                          <ul className="govuk-list">
                            <li>
                              <Link
                                as={`/statistics/${publication.slug}`}
                                to={`/statistics/publication?publication=${publication.slug}`}
                              >
                                Go to publication
                              </Link>
                            </li>
                            <li>
                              <ButtonText onClick={this.handlePermalinkClick}>
                                Create permanent link
                              </ButtonText>
                            </li>
                            <li>
                              <DownloadCsvButton
                                publicationSlug={publication.slug}
                                meta={subjectMeta}
                                filters={filters}
                                indicators={indicators}
                                locations={locationsList}
                                timePeriods={timePeriodRange}
                                results={tableData}
                              />
                            </li>

                            <li>
                              <a href="#api">Access developer API</a>
                            </li>
                            <li>
                              <Link
                                as={`/methodology/${publication.slug}`}
                                to={`/methodology/methodology?methodology=${publication.slug}`}
                              >
                                Go to methodology
                              </Link>
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

              <PreviousStepModalConfirm />
            </>
          )}
        </ConfirmContextProvider>
      </Page>
    );
  }
}

export default TableToolPage;
