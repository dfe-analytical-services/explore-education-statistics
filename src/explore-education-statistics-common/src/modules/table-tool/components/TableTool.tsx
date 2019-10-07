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
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@common/modules/full-table/types/filters';
import parseYearCodeTuple from '@common/modules/full-table/utils/TimePeriod';
import mapValues from 'lodash/mapValues';
import React, {createRef, ReactNode} from 'react';
import getDefaultTableHeaderConfig from '@common/modules/full-table/utils/tableHeaders';
import {FullTable} from '@common/modules/full-table/types/fullTable';
import {mapFullTable} from '@common/modules/full-table/utils/mapPermalinks';
import FiltersForm, {
  FilterFormSubmitHandler,
} from '@common/modules/table-tool/components/FiltersForm';
import LocationFiltersForm, {
  LocationFiltersFormSubmitHandler, LocationsFormValues,
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
  publication?: Publication;
  createdTable: FullTable;
  query: TableDataQuery;
  tableHeaders: TableHeadersFormValues;
}

interface Props {
  themeMeta: ThemeMeta[];
  publicationId?: string;
  releaseId?: string;

  finalStepExtra?: (props: FinalStepProps) => ReactNode;
  finalStepHeading?: string;

  initialQuery?: TableDataQuery;
}

interface DateRangeState {
  startYear?: number;
  startCode?: string;
  endYear?: number;
  endCode?: string;
}

const createQuery = (
  filters: Dictionary<CategoryFilter[]>,
  indicators: Indicator[],
  {
    subjectId,
    startYear,
    startCode,
    endYear,
    endCode,
    locations,
  }: {
    subjectId: string;
    locations: Dictionary<LocationFilter[]>;
  } & DateRangeState,
): TableDataQuery => {
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

const TableTool = ({
  themeMeta,
  publicationId,
  releaseId,
  finalStepExtra,
  finalStepHeading,
  initialQuery,
}: Props) => {
  const dataTableRef = createRef<HTMLTableElement>();

  const [subjects, setSubjects] = React.useState<PublicationSubject[]>([]);

  const [publication, setPublication] = React.useState<Publication>();

  const [subjectId, setSubjectId] = React.useState<string>('');

  const [subjectMeta, setSubjectMeta] = React.useState<PublicationSubjectMeta>({
    timePeriod: {
      hint: '',
      legend: '',
      options: [],
    },
    locations: {},
    indicators: {},
    filters: {},
  });

  const [locations, setLocations] = React.useState<Dictionary<LocationFilter[]>>({});

  const [dateRange, setDateRange] = React.useState<DateRangeState>({});

  const [tableHeaders, setTableHeaders] = React.useState<TableHeadersFormValues>({
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  });

  const [createdTable, setCreatedTable] = React.useState<FullTable>();

  const [query, setQuery] = React.useState<TableDataQuery>();

  const [initialStep, setInitialStep] = React.useState(1);

  const mapLocations = (selectedLocations : LocationsFormValues) =>  mapValuesWithKeys(selectedLocations, (locationLevel, locationOptions) =>
      locationOptions
        .map(location =>
          subjectMeta.locations[locationLevel].options.find(
            option => option.value === location,
          ),
        )
        .filter(option => typeof option !== 'undefined')
        .map(
          option => new LocationFilter(option as FilterOption, locationLevel),
        ),
    );

  React.useEffect(() => {

    console.log(initialQuery);

    if (initialQuery) {

      const mappedLocations = mapLocations();

      setSubjectId(initialQuery.subjectId);
      setLocations(initialQuery.)
      setInitialStep(2);
    }

  }, [initialQuery]);

  const handlePublicationFormSubmit: PublicationFormSubmitHandler = async ({
    publicationId: selectedPublicationId,
  }) => {
    const selectedPublication = themeMeta
      .flatMap(option => option.topics)
      .flatMap(option => option.publications)
      .find(option => option.id === selectedPublicationId);

    if (!selectedPublication) {
      return;
    }

    const {
      subjects: publicationSubjects,
    } = await tableBuilderService.getPublicationMeta(selectedPublicationId);

    setSubjects(publicationSubjects);

    setPublication(selectedPublication);
  };

  const handlePublicationSubjectFormSubmit: PublicationSubjectFormSubmitHandler = async ({
    subjectId: selectedSubjectId,
  }) => {
    const selectedSubjectMeta = await tableBuilderService.getPublicationSubjectMeta(
      selectedSubjectId,
    );

    setSubjectId(selectedSubjectId);
    setSubjectMeta(selectedSubjectMeta);
  };

  React.useEffect(() => {
    if (releaseId) {
      tableBuilderService
        .getReleaseMeta(releaseId)
        .then(({subjects: releaseSubjects}) => {
          setSubjects(releaseSubjects);
        });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [releaseId]);

  const handleLocationFiltersFormSubmit: LocationFiltersFormSubmitHandler = async ({
    locations: selectedLocations,
  }) => {
    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
        ...selectedLocations,
        subjectId,
      },
    );

    setSubjectMeta({
      ...subjectMeta,
      timePeriod: selectedSubjectMeta.timePeriod,
    });

    setLocations(mapLocations(selectedLocations));
  };

  const handleTimePeriodFormSubmit: TimePeriodFormSubmitHandler = async values => {
    const [startYear, startCode] = parseYearCodeTuple(values.start);
    const [endYear, endCode] = parseYearCodeTuple(values.end);

    const selectedSubjectMeta = await tableBuilderService.filterPublicationSubjectMeta(
      {
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
      },
    );

    setSubjectMeta({
      ...subjectMeta,
      filters: selectedSubjectMeta.filters,
    });

    setDateRange({
      startYear,
      startCode,
      endYear,
      endCode,
    });
  };

  const handleFiltersFormSubmit: FilterFormSubmitHandler = async values => {
    const {startYear, startCode, endYear, endCode} = dateRange;

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

    const createdQuery: TableDataQuery = createQuery(filters, indicators, {
      subjectId,
      locations,
      ...dateRange,
    });

    let unmappedCreatedTable;

    if (releaseId) {
      unmappedCreatedTable = await tableBuilderService.getTableDataForRelease(
        createdQuery,
        releaseId,
      );
    } else {
      unmappedCreatedTable = await tableBuilderService.getTableData(
        createdQuery,
      );
    }

    const table = mapFullTable(unmappedCreatedTable);

    setCreatedTable(table);

    setTableHeaders(getDefaultTableHeaderConfig(table.subjectMeta));

    setQuery(createdQuery);
  };

  return (
    <ConfirmContextProvider>
      {({askConfirm}) => (
        <>
          <Wizard
            initialStep={initialStep}
            id="tableTool-steps"
            onStepChange={async (nextStep, previousStep) => {
              if (nextStep < previousStep) {
                const confirmed = await askConfirm();
                return confirmed ? nextStep : previousStep;
              }

              return nextStep;
            }}
          >
            {releaseId === undefined && (
              <WizardStep>
                {stepProps => (
                  <PublicationForm
                    {...stepProps}
                    publicationId={publicationId}
                    publicationTitle={publication ? publication.title : ''}
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
                  options={subjects}
                  initialSubjectId={initialQuery && initialQuery.subjectId}
                  onSubmit={handlePublicationSubjectFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <LocationFiltersForm
                  {...stepProps}
                  options={subjectMeta.locations}
                  onSubmit={handleLocationFiltersFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <TimePeriodForm
                  {...stepProps}
                  options={subjectMeta.timePeriod.options}
                  onSubmit={handleTimePeriodFormSubmit}
                />
              )}
            </WizardStep>
            <WizardStep>
              {stepProps => (
                <FiltersForm
                  {...stepProps}
                  onSubmit={handleFiltersFormSubmit}
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
                        setTableHeaders(tableHeaderConfig);

                        if (dataTableRef.current) {
                          dataTableRef.current.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start',
                          });
                        }
                      }}
                    />
                    {createdTable ? (
                      <TimePeriodDataTable
                        ref={dataTableRef}
                        fullTable={createdTable}
                        tableHeadersConfig={tableHeaders}
                      />
                    ) : null}
                  </div>

                  {createdTable &&
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
};

export default TableTool;
