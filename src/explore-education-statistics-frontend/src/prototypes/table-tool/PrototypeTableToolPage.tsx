import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import tableBuilderService, {
  DataTableResult,
} from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import PageTitle from '@frontend/components/PageTitle';
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
import TimePeriodForm from '@frontend/prototypes/table-tool/components/TimePeriodForm';
import mapOptionValues from '@frontend/prototypes/table-tool/components/utils/mapOptionValues';
import Wizard from '@frontend/prototypes/table-tool/components/Wizard';
import WizardStep from '@frontend/prototypes/table-tool/components/WizardStep';
import mapValues from 'lodash/mapValues';
import React, { Component, createRef } from 'react';
import PublicationForm from './components/PublicationForm';
import PublicationSubjectForm from './components/PublicationSubjectForm';

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

  private publicationSubjectRef = createRef<HTMLLIElement>();

  private filtersRef = createRef<HTMLElement>();

  private dataTableRef = createRef<HTMLElement>();

  private locationFiltersRef = createRef<HTMLElement>();

  private handlePublicationFormSubmit = async ({
    publicationId,
    publicationName,
  }: {
    publicationId: string;
    publicationName: string;
  }) => {
    if (!publicationId) {
      return;
    }

    if (this.publicationSubjectRef.current) {
      this.publicationSubjectRef.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
      this.publicationSubjectRef.current.focus();
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
      () => {
        if (this.publicationSubjectRef.current) {
          this.publicationSubjectRef.current.scrollIntoView({
            behavior: 'smooth',
            block: 'start',
          });
          this.publicationSubjectRef.current.focus();
        }
      },
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

        <Wizard id="tableTool-steps">
          <WizardStep>
            {stepProps => (
              <PublicationForm
                {...stepProps}
                options={defaultPublicationOptions}
                onSubmit={this.handlePublicationFormSubmit}
              />
            )}
          </WizardStep>
          <WizardStep ref={this.publicationSubjectRef}>
            {stepProps => (
              <PublicationSubjectForm
                {...stepProps}
                options={publicationSubjectSpec.subjects}
                onSubmit={values =>
                  this.setState(
                    {
                      publicationSubjectName: values.publicationSubjectName,
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
              />
            )}
          </WizardStep>
          <WizardStep>
            {stepProps =>
              publicationSubjectName && (
                <LocationFiltersForm
                  {...stepProps}
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
              )
            }
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <TimePeriodForm
                {...stepProps}
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
            )}
          </WizardStep>
          <WizardStep>
            {stepProps => (
              <FiltersForm
                {...stepProps}
                onSubmit={this.handleFilterFormSubmit}
                specification={metaSpecification}
              />
            )}
          </WizardStep>
          <WizardStep>
            {tableData.length > 0 && (
              <TimePeriodDataTable
                filters={filters}
                indicators={indicators}
                timePeriods={timePeriods}
                results={tableData}
              />
            )}

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
          </WizardStep>
        </Wizard>
      </PrototypePage>
    );
  }
}

export default PrototypeTableToolPage;
