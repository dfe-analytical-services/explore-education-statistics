import { ConfirmContextProvider } from '@common/context/ConfirmContext';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption,
  IndicatorOption,
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import { Dictionary } from '@common/types/util';
import PreviousStepModalConfirm from '@common/modules/table-tool/components/PreviousStepModalConfirm';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@common/modules/full-table/types/filters';
import parseYearCodeTuple from '@common/modules/full-table/utils/TimePeriod';
import mapValues from 'lodash/mapValues';
import React, { Component, createRef, ReactNode } from 'react';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import { FullTable } from '@common/modules/full-table/types/fullTable';
import { mapFullTable } from '@common/modules/full-table/utils/mapPermalinks';
import FiltersForm, {
  FilterFormSubmitHandler,
} from '@common/modules/table-tool/components/FiltersForm';
import LocationFiltersForm, {
  LocationFiltersFormSubmitHandler,
} from '@common/modules/table-tool/components/LocationFiltersForm';
import PublicationForm, {
  PublicationFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationForm';
import PublicationSubjectForm, {
  PublicationSubjectFormSubmitHandler,
} from '@common/modules/table-tool/components/PublicationSubjectForm';
import TableHeadersForm, {
  TableHeadersFormValues,
} from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import TimePeriodForm, {
  TimePeriodFormSubmitHandler,
} from '@common/modules/table-tool/components/TimePeriodForm';
import mapOptionValues from '@common/modules/table-tool/components/utils/mapOptionValues';
import Wizard from '@common/modules/table-tool/components/Wizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';

interface Publication {
  id: string;
  title: string;
  slug: string;
}

interface FinalStepProps {
  publication: Publication;
  createdTable: FullTable;
  query: TableDataQuery;
  tableHeaders: TableHeadersFormValues;
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId: string;
  finalStepExtra?: (props: FinalStepProps) => ReactNode;
  finalStepHeading?: string;
}

interface State {
  createdTable?: FullTable;
  startYear?: number;
  startCode?: string;
  endYear?: number;
  endCode?: string;
  locations: Dictionary<LocationFilter[]>;
  filters: Dictionary<CategoryFilter[]>;
  indicators: Indicator[];
  publication?: Publication;
  subjects: PublicationSubject[];
  subjectId: string;
  subjectMeta: PublicationSubjectMeta;
  tableHeaders: TableHeadersFormValues;
  query?: TableDataQuery;
}

class TableTool extends Component<Props, State> {
  public state: State = {
    filters: {},
    indicators: [],
    locations: {},
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
    tableHeaders: {
      columnGroups: [],
      columns: [],
      rowGroups: [],
      rows: [],
    },
  };

  private dataTableRef = createRef<HTMLTableElement>();

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
    });
  };

  private handlePublicationSubjectFormSubmit: PublicationSubjectFormSubmitHandler = async ({
    subjectId,
  }) => {
    const subjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      subjectId,
    );

    this.setState({
      subjectMeta,
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
    const { startYear, startCode, endYear, endCode, subjectMeta } = this.state;

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

    const query: TableDataQuery = this.createQuery(filters, indicators);

    const unmappedCreatedTable = await tableBuilderService.getTableData(query);

    const createdTable = mapFullTable(unmappedCreatedTable);

    this.setState({
      createdTable,
      filters,
      indicators,
      tableHeaders: getDefaultTableHeaderConfig(createdTable.subjectMeta),
      query,
    });
  };

  public render() {
    const {
      themeMeta,
      publicationId,
      finalStepExtra,
      finalStepHeading,
    } = this.props;
    const {
      createdTable,
      publication,
      subjectMeta,
      subjects,
      tableHeaders,
      indicators,
      filters,
      query,
    } = this.state;

    return (
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
                    publicationTitle={publication ? publication.title : ''}
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
                      {finalStepHeading || 'Explore data'}
                    </WizardStepHeading>

                    <div className="govuk-!-margin-bottom-4">
                      <TableHeadersForm
                        initialValues={tableHeaders}
                        onSubmit={tableHeaderConfig => {
                          this.setState({
                            tableHeaders: tableHeaderConfig,
                          });

                          if (this.dataTableRef.current) {
                            this.dataTableRef.current.scrollIntoView({
                              behavior: 'smooth',
                              block: 'start',
                            });
                          }
                        }}
                      />
                      {createdTable ? (
                        <TimePeriodDataTable
                          ref={this.dataTableRef}
                          fullTable={createdTable}
                          tableHeadersConfig={tableHeaders}
                        />
                      ) : null}
                    </div>

                    {publication &&
                      createdTable &&
                      finalStepExtra &&
                      query &&
                      finalStepExtra({
                        createdTable,
                        publication,
                        tableHeaders,
                        query,
                      })}
                  </>
                )}
              </WizardStep>
            </Wizard>

            <PreviousStepModalConfirm />
          </>
        )}
      </ConfirmContextProvider>
    );
  }
}

export default TableTool;
