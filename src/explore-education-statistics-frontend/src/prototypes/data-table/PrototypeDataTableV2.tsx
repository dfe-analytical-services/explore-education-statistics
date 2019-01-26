import React, { ChangeEvent, Component, createRef } from 'react';
import FormCheckboxGroup from '../../components/FormCheckboxGroup';
import { RadioChangeEventHandler } from '../../components/FormRadio';
import FormRadioGroup from '../../components/FormRadioGroup';
import PageHeading from '../../components/PageHeading';
import Tabs from '../../components/Tabs';
import TabsSection from '../../components/TabsSection';
import PrototypePage from '../components/PrototypePage';
import PrototypeAbsenceGeneralChart from './charts/PrototypeAbsenceGeneralChart';
import PrototypeAbsenceRateChart from './charts/PrototypeAbsenceRateChart';
import PrototypeFixedPeriodExclusionsChart from './charts/PrototypeFixedPeriodExclusionsChart';
import PrototypePermanentExclusionsChart from './charts/PrototypePermanentExclusionsChart';
import FilterMenuRadios, {
  MenuChangeEventHandler,
  MenuOption,
} from './components/FilterMenuRadios';
import { allTableData as exclusionTableData } from './test-data/exclusionsDataV1';
import { allTableData as absenceTableData } from './test-data/pupilAbsenceDataV1';

type DataToggles = 'CHARTS_TABLES' | 'CHARTS' | 'TABLES' | null;

const allTableData: any = {
  exclusions: exclusionTableData,
  pupilAbsence: absenceTableData,
};

type PupilAbsenceGeneralFilters = 'enrolments' | 'schools';
type PupilAbsenceSessionsFilters =
  | 'authorisedRate'
  | 'overallRate'
  | 'unauthorisedRate';
type ExclusionsExclusionsFilters =
  | 'permanent'
  | 'permanentRate'
  | 'fixedPeriod'
  | 'fixedPeriodRate';

interface State {
  chartData: {
    [key: string]: number;
  }[];
  dataToggle: DataToggles;
  menuOption?: MenuOption | null;
  filters: {
    exclusions: {
      exclusions: { [key in ExclusionsExclusionsFilters]: boolean };
    };
    pupilAbsence: {
      general: { [key in PupilAbsenceGeneralFilters]: boolean };
      sessions: { [key in PupilAbsenceSessionsFilters]: boolean };
    };
  };
  tableData: string[][];
}

class PrototypeDataTableV2 extends Component<{}, State> {
  public state: State = {
    chartData: [],
    dataToggle: null,
    filters: {
      exclusions: {
        exclusions: {
          fixedPeriod: false,
          fixedPeriodRate: false,
          permanent: false,
          permanentRate: false,
        },
      },
      pupilAbsence: {
        general: {
          enrolments: false,
          schools: false,
        },
        sessions: {
          authorisedRate: false,
          overallRate: false,
          unauthorisedRate: false,
        },
      },
    },
    menuOption: null,
    tableData: [],
  };

  private dataTableRef = createRef<HTMLDivElement>();

  private handleMenuChange: MenuChangeEventHandler = menuOption => {
    this.setState(
      {
        menuOption,
        filters: {
          exclusions: {
            exclusions: {
              fixedPeriod: false,
              fixedPeriodRate: false,
              permanent: false,
              permanentRate: false,
            },
          },
          pupilAbsence: {
            general: {
              enrolments: false,
              schools: false,
            },
            sessions: {
              authorisedRate: false,
              overallRate: false,
              unauthorisedRate: false,
            },
          },
        },
        tableData: [],
      },
      () => {
        if (this.dataTableRef.current) {
          this.dataTableRef.current.scrollIntoView({ behavior: 'smooth' });
        }
      },
    );
  };

  private createTableData(filters: {}) {
    return Object.entries(filters).flatMap(([publicationKey, publication]) => {
      return Object.entries(publication)
        .flatMap(([groupKey, group]) => {
          return Object.entries(group)
            .filter(([_, isChecked]) => isChecked)
            .map(([filterKey]) => {
              return allTableData[publicationKey][groupKey][filterKey];
            });
        })
        .filter(row => row.length > 0);
    });
  }

  private handleAllFilterCheckboxChange = (
    publicationKey: 'pupilAbsence' | 'exclusions',
    filterGroupKey: 'exclusions' | 'general' | 'sessions',
    event: ChangeEvent<HTMLInputElement>,
  ) => {
    const filterGroup = this.state.filters[publicationKey] as any;
    const subFilterGroup = filterGroup[filterGroupKey];

    const filters = {
      ...this.state.filters,
      [publicationKey]: {
        ...filterGroup,
        [filterGroupKey]: Object.keys(subFilterGroup).reduce((acc, key) => {
          return {
            ...acc,
            [key]: event.target.checked,
          };
        }, {}),
      },
    };

    this.setState({
      filters,
      tableData: this.createTableData(filters),
    });
  };

  private handleFilterCheckboxChange = (
    publicationKey: 'pupilAbsence' | 'exclusions',
    filterGroupKey: 'exclusions' | 'general' | 'sessions',
    event: ChangeEvent<HTMLInputElement>,
  ) => {
    const filterGroup = this.state.filters[publicationKey] as any;
    const subFilterGroup = filterGroup[filterGroupKey];

    const filters = {
      ...this.state.filters,
      [publicationKey]: {
        ...filterGroup,
        [filterGroupKey]: {
          ...subFilterGroup,
          [event.target.value]: event.target.checked,
        },
      },
    };

    this.setState({
      filters,
      tableData: this.createTableData(filters),
    });
  };

  private handleRadioChange: RadioChangeEventHandler<{
    value: DataToggles;
  }> = event => {
    this.setState({ dataToggle: event.target.value });
  };

  private hasSessionsFilters() {
    return (
      Object.values(this.state.filters.pupilAbsence.sessions).indexOf(true) > -1
    );
  }

  private hasGeneralFilters() {
    return (
      Object.values(this.state.filters.pupilAbsence.general).indexOf(true) > -1
    );
  }

  private hasPermanentExclusionFilters() {
    return (
      this.state.filters.exclusions.exclusions.permanent ||
      this.state.filters.exclusions.exclusions.permanentRate
    );
  }

  private hasFixedPeriodExclusionFilters() {
    return (
      this.state.filters.exclusions.exclusions.fixedPeriod ||
      this.state.filters.exclusions.exclusions.fixedPeriodRate
    );
  }

  private hasAnyFilters() {
    return Object.values(this.state.filters).map(filterGroup =>
      Object.values(filterGroup).some(filter => filter.size > 0),
    );
  }

  public render() {
    const { filters } = this.state;

    return (
      <PrototypePage
        breadcrumbs={[
          { text: 'Education training and skills' },
          { text: 'National level' },
          { text: 'Explore statistics' },
        ]}
        wide
      >
        <PageHeading caption="National level" heading="Explore statistics" />

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
            <FilterMenuRadios onChange={this.handleMenuChange} />
          </div>
        </div>

        {(this.state.menuOption === 'EXCLUSIONS' ||
          this.state.menuOption === 'PUPIL_ABSENCE') && (
          <div ref={this.dataTableRef}>
            {this.state.menuOption === 'PUPIL_ABSENCE' && (
              <h2>
                2. Explore statistics from 'Pupil absence'
                <span className="govuk-hint">
                  Select any statistics you are interested in from the
                  checkboxes below
                </span>
              </h2>
            )}

            {this.state.menuOption === 'EXCLUSIONS' && (
              <h2>
                2. Explore statistics from 'Exclusions'
                <span className="govuk-hint">
                  Select any statistics you are interested in from the
                  checkboxes below
                </span>
              </h2>
            )}

            <div className="govuk-grid-row">
              <div className="govuk-grid-column-one-quarter">
                {this.state.menuOption === 'PUPIL_ABSENCE' && (
                  <>
                    <div className="govuk-form-group">
                      <FormCheckboxGroup
                        checkedValues={this.state.filters.pupilAbsence.general}
                        name="pupilAbsenceGeneral"
                        legend="General"
                        onAllChange={this.handleAllFilterCheckboxChange.bind(
                          this,
                          'pupilAbsence',
                          'general',
                        )}
                        onChange={this.handleFilterCheckboxChange.bind(
                          this,
                          'pupilAbsence',
                          'general',
                        )}
                        options={[
                          {
                            id: 'general-enrolments',
                            label: 'Enrolments',
                            value: 'enrolments',
                          },
                          {
                            id: 'general-schools',
                            label: 'Schools',
                            value: 'schools',
                          },
                        ]}
                      />
                    </div>

                    <div className="govuk-form-group">
                      <FormCheckboxGroup
                        checkedValues={this.state.filters.pupilAbsence.sessions}
                        name="pupilAbsenceSessions"
                        legend="Sessions absent"
                        onAllChange={this.handleAllFilterCheckboxChange.bind(
                          this,
                          'pupilAbsence',
                          'sessions',
                        )}
                        onChange={this.handleFilterCheckboxChange.bind(
                          this,
                          'pupilAbsence',
                          'sessions',
                        )}
                        options={[
                          {
                            id: 'sessions-authorised-rate',
                            label: 'Authorised rate',
                            value: 'authorisedRate',
                          },
                          {
                            id: 'sessions-overall-rate',
                            label: 'Overall rate',
                            value: 'overallRate',
                          },
                          {
                            id: 'sessions-unauthorised-rate',
                            label: 'Unauthorised rate',
                            value: 'unauthorisedRate',
                          },
                        ]}
                      />
                    </div>
                  </>
                )}

                {this.state.menuOption === 'EXCLUSIONS' && (
                  <FormCheckboxGroup
                    checkedValues={this.state.filters.exclusions.exclusions}
                    name="exclusions"
                    legend="Exclusions"
                    onAllChange={this.handleAllFilterCheckboxChange.bind(
                      this,
                      'exclusions',
                      'exclusions',
                    )}
                    onChange={this.handleFilterCheckboxChange.bind(
                      this,
                      'exclusions',
                      'exclusions',
                    )}
                    options={[
                      {
                        id: 'permanent-exclusions',
                        label: 'Permanent exclusions',
                        value: 'permanent',
                      },
                      {
                        id: 'permanent-exclusions-rate',
                        label: 'Permanent exclusion rate',
                        value: 'permanentRate',
                      },
                      {
                        id: 'fixed-period-exclusions',
                        label: 'Fixed period exclusions',
                        value: 'fixedPeriod',
                      },
                      {
                        id: 'fixed-period-exclusions-rate',
                        label: 'Fixed period exclusion rate',
                        value: 'fixedPeriodRate',
                      },
                    ]}
                  />
                )}
              </div>

              {this.hasAnyFilters() && (
                <div className="govuk-grid-column-three-quarters">
                  <FormRadioGroup
                    checkedValue={this.state.dataToggle}
                    inline
                    name="dataToggle"
                    legend="What do you want to see?"
                    legendSize="m"
                    onChange={this.handleRadioChange as any}
                    options={[
                      {
                        id: 'chartsAndTable',
                        label: 'Charts and table',
                        value: 'CHARTS_TABLES',
                      },
                      { id: 'charts', label: 'Charts', value: 'CHARTS' },
                      { id: 'table', label: 'Table', value: 'TABLES' },
                    ]}
                  />

                  {this.state.dataToggle && (
                    <>
                      <p>View by:</p>

                      <Tabs>
                        <TabsSection id="years" title="Academic years">
                          <>
                            {(this.state.dataToggle === 'CHARTS_TABLES' ||
                              this.state.dataToggle === 'CHARTS') && (
                              <div className="govuk-grid-row">
                                {this.state.menuOption === 'PUPIL_ABSENCE' && (
                                  <>
                                    {this.hasGeneralFilters() && (
                                      <div className="govuk-grid-column-one-half">
                                        <p>General</p>

                                        <PrototypeAbsenceGeneralChart
                                          enrolments={
                                            filters.pupilAbsence.general
                                              .enrolments
                                          }
                                          schools={
                                            filters.pupilAbsence.general.schools
                                          }
                                        />
                                      </div>
                                    )}
                                    {this.hasSessionsFilters() && (
                                      <div className="govuk-grid-column-one-half">
                                        <p>Sessions absent</p>

                                        <PrototypeAbsenceRateChart
                                          authorised={
                                            filters.pupilAbsence.sessions
                                              .authorisedRate
                                          }
                                          unauthorised={
                                            filters.pupilAbsence.sessions
                                              .unauthorisedRate
                                          }
                                          overall={
                                            filters.pupilAbsence.sessions
                                              .overallRate
                                          }
                                        />
                                      </div>
                                    )}
                                  </>
                                )}
                                {this.state.menuOption === 'EXCLUSIONS' && (
                                  <>
                                    {this.hasPermanentExclusionFilters() && (
                                      <div className="govuk-grid-column-one-half">
                                        <p>Permanent exclusions</p>

                                        <PrototypePermanentExclusionsChart
                                          exclusions={
                                            filters.exclusions.exclusions
                                              .permanent
                                          }
                                          exclusionsRate={
                                            filters.exclusions.exclusions
                                              .permanentRate
                                          }
                                        />
                                      </div>
                                    )}
                                    {this.hasFixedPeriodExclusionFilters() && (
                                      <div className="govuk-grid-column-one-half">
                                        <p>Fixed period exclusions</p>

                                        <PrototypeFixedPeriodExclusionsChart
                                          exclusions={
                                            filters.exclusions.exclusions
                                              .fixedPeriod
                                          }
                                          exclusionsRate={
                                            filters.exclusions.exclusions
                                              .fixedPeriodRate
                                          }
                                        />
                                      </div>
                                    )}
                                  </>
                                )}
                              </div>
                            )}

                            {this.hasAnyFilters() && (
                              <>
                                <hr />

                                {(this.state.dataToggle === 'CHARTS_TABLES' ||
                                  this.state.dataToggle === 'TABLES') && (
                                  <table className="govuk-table">
                                    <caption>
                                      Comparing statistics between 2012 and 2017
                                    </caption>
                                    <thead>
                                      <tr>
                                        <th />
                                        <th scope="col">2012/13</th>
                                        <th scope="col">2013/14</th>
                                        <th scope="col">2014/15</th>
                                        <th scope="col">2015/16</th>
                                        <th scope="col">2016/17</th>
                                      </tr>
                                    </thead>
                                    <tbody>
                                      {this.state.tableData.map(
                                        ([firstCell, ...cells], rowIndex) => (
                                          <tr key={rowIndex}>
                                            <td scope="row">{firstCell}</td>
                                            {cells.map((cell, cellIndex) => (
                                              <td key={cellIndex}>{cell}</td>
                                            ))}
                                          </tr>
                                        ),
                                      )}
                                    </tbody>
                                  </table>
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
                              </>
                            )}
                          </>
                        </TabsSection>
                        <TabsSection id="school-type" title="School type">
                          {null}
                        </TabsSection>
                        <TabsSection id="geography" title="Geography">
                          {null}
                        </TabsSection>
                        <TabsSection
                          id="demographics-characteristics"
                          title="Demographics/characteristics"
                        >
                          {null}
                        </TabsSection>
                      </Tabs>
                    </>
                  )}
                </div>
              )}
            </div>
          </div>
        )}
      </PrototypePage>
    );
  }
}

export default PrototypeDataTableV2;
