import {ConfirmContextProvider} from '@common/context/ConfirmContext';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  FilterOption,
  IndicatorOption,
  PublicationSubject,
  PublicationSubjectMeta,
  TableDataQuery,
  ThemeMeta,
} from '@common/modules/full-table/services/tableBuilderService';
import {Dictionary} from '@common/types/util';
import PreviousStepModalConfirm from '@common/modules/table-tool/components/PreviousStepModalConfirm';
import {CategoryFilter, Indicator, LocationFilter,} from '@common/modules/full-table/types/filters';
import parseYearCodeTuple from '@common/modules/full-table/utils/TimePeriod';
import mapValues from 'lodash/mapValues';
import React, {createRef, ReactNode} from 'react';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import {FullTable} from '@common/modules/full-table/types/fullTable';
import {mapFullTable} from '@common/modules/full-table/utils/mapPermalinks';
import FiltersForm, {FilterFormSubmitHandler,} from '@common/modules/table-tool/components/FiltersForm';
import LocationFiltersForm, {LocationFiltersFormSubmitHandler,} from '@common/modules/table-tool/components/LocationFiltersForm';
import PublicationForm, {PublicationFormSubmitHandler,} from '@common/modules/table-tool/components/PublicationForm';
import PublicationSubjectForm, {PublicationSubjectFormSubmitHandler,} from '@common/modules/table-tool/components/PublicationSubjectForm';
import TableHeadersForm, {TableHeadersFormValues,} from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import TimePeriodForm, {TimePeriodFormSubmitHandler,} from '@common/modules/table-tool/components/TimePeriodForm';
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
  fixedPublicationId?: boolean;

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
  publication?: Publication;
  subjects: PublicationSubject[];
  subjectId: string;
  subjectMeta: PublicationSubjectMeta;
  tableHeaders: TableHeadersFormValues;
  query?: TableDataQuery;
}

const TableTool = ({
  themeMeta,
  publicationId,
  fixedPublicationId = false,
  finalStepExtra,
  finalStepHeading,
}: Props) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [state, setState] = React.useState<State>({
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
  });

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publicationId: pid,
  }) => {
    const publication = themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.id === pid);

    if (!publication) {
      return;
    }

    const { subjects } = await tableBuilderService.getPublicationMeta(pid);

    setState({
      ...state,
      publication,
      subjects,
    });
  };



  const handlePublicationSubjectFormSubmit: PublicationSubjectFormSubmitHandler = async ({
    subjectId,
  }) => {
    const subjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      subjectId,
    );

    setState({
      ...state,
      subjectMeta,
      subjectId,
    });
  };

  React.useEffect(() => {
    if (fixedPublicationId === true) {
      handlePublicationFormSubmit({ publicationId });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [publicationId, fixedPublicationId]);

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations,
  }) => {
    const { subjectId } = state;

    const subjectMeta = await tableBuilderService.filterPublicationSubjectMeta({
      ...locations,
      subjectId,
    });

    setState({
      ...state,
      subjectMeta: {
        ...state.subjectMeta,
        timePeriod: subjectMeta.timePeriod,
      },
      locations: mapValuesWithKeys(
        locations,
        (locationLevel, locationOptions) =>
          locationOptions
            .map(location =>
              state.subjectMeta.locations[locationLevel].options.find(
                option => option.value === location,
              ),
            )
            .filter(option => typeof option !== 'undefined')
            .map(
              option =>
                new LocationFilter(option as FilterOption, locationLevel),
            ),
      ),
    });
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const { subjectId, locations } = state;

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

    setState({
      ...state,
      startYear,
      startCode,
      endYear,
      endCode,
      subjectMeta: {
        ...state.subjectMeta,
        filters: subjectMeta.filters,
      },
    });
  };

  const createQuery = (
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
    } = state;

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

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async values => {
    const { startYear, startCode, endYear, endCode, subjectMeta } = state;

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

    const query: TableDataQuery = createQuery(filters, indicators);

    const unmappedCreatedTable = await tableBuilderService.getTableData(query);

    const createdTable = mapFullTable(unmappedCreatedTable);

    setState({
      ...state,
      createdTable,
      tableHeaders: getDefaultTableHeaderConfig(createdTable.subjectMeta),
      query,
    });
  };

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
            {fixedPublicationId !== true && (
              <WizardStep>
                {stepProps => (
                  <PublicationForm
                    {...stepProps}
                    publicationId={publicationId}
                    publicationTitle={
                      state.publication ? state.publication.title : ''
                    }
                    options={themeMeta}
                    onSubmit={handlePublicationFormSubmit}
                  />
                )}
              </WizardStep>
            )}
            <WizardStep>
              {stepProps => (
                <PublicationSubjectForm
                  {...stepProps}
                  options={state.subjects}
                  onSubmit={handlePublicationSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  options={state.subjectMeta.locations}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  options={state.subjectMeta.timePeriod.options}
                  onSubmit={handleTimePeriodFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <FiltersForm
                  {...stepProps}
                  onSubmit={handleFiltersFormSubmit}
                  subjectMeta={state.subjectMeta}
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
                      initialValues={state.tableHeaders}
                      onSubmit={tableHeaderConfig => {
                        setState({
                          ...state,
                          tableHeaders: tableHeaderConfig,
                        });

                        if (dataTableRef.current) {
                          dataTableRef.current.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start',
                          });
                        }
                      }}
                    />
                    {state.createdTable ? (
                      <TimePeriodDataTable
                        ref={dataTableRef}
                        fullTable={state.createdTable}
                        tableHeadersConfig={state.tableHeaders}
                      />
                    ) : null}
                  </div>

                  {state.publication &&
                    state.createdTable &&
                    finalStepExtra &&
                    state.query &&
                    finalStepExtra({
                      createdTable: state.createdTable,
                      publication: state.publication,
                      tableHeaders: state.tableHeaders,
                      query: state.query,
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
};

export default TableTool;
