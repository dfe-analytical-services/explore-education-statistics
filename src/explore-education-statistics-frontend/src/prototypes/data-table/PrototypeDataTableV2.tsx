import React, { ChangeEventHandler, Component, createRef } from 'react';
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
import { allTableData as absenceTableData } from './test-data/absenceRateData';
import { allTableData as exclusionTableData } from './test-data/exclusionRateData';

type DataToggles = 'CHARTS_TABLES' | 'CHARTS' | 'TABLES' | null;

const allTableData: any = {
  ...absenceTableData,
  ...exclusionTableData,
};

// tslint:disable-next-line:interface-over-type-literal
type Filters = {
  GENERAL_ALL: boolean;
  FIXED_PERIOD_EXCLUSIONS: boolean;
  FIXED_PERIOD_EXCLUSIONS_RATE: boolean;
  EXCLUSIONS_ALL: boolean;
  GENERAL_ENROLMENTS: boolean;
  GENERAL_SCHOOLS: boolean;
  PERMANENT_EXCLUSIONS: boolean;
  PERMANENT_EXCLUSIONS_RATE: boolean;
  SESSIONS_ALL: boolean;
  SESSIONS_AUTHORISED_RATE: boolean;
  SESSIONS_OVERALL_RATE: boolean;
  SESSIONS_UNAUTHORISED_RATE: boolean;
};

interface State {
  chartData: {
    [key: string]: number;
  }[];
  dataToggle: DataToggles;
  menuOption?: MenuOption | null;
  filters: Filters;
  tableData: string[][];
}

class PrototypeDataTableV2 extends Component<{}, State> {
  public state: State = {
    chartData: [],
    dataToggle: null,
    filters: {
      EXCLUSIONS_ALL: false,
      FIXED_PERIOD_EXCLUSIONS: false,
      FIXED_PERIOD_EXCLUSIONS_RATE: false,
      GENERAL_ALL: false,
      GENERAL_ENROLMENTS: false,
      GENERAL_SCHOOLS: false,
      PERMANENT_EXCLUSIONS: false,
      PERMANENT_EXCLUSIONS_RATE: false,
      SESSIONS_ALL: false,
      SESSIONS_AUTHORISED_RATE: false,
      SESSIONS_OVERALL_RATE: false,
      SESSIONS_UNAUTHORISED_RATE: false,
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
          EXCLUSIONS_ALL: false,
          FIXED_PERIOD_EXCLUSIONS: false,
          FIXED_PERIOD_EXCLUSIONS_RATE: false,
          GENERAL_ALL: false,
          GENERAL_ENROLMENTS: false,
          GENERAL_SCHOOLS: false,
          PERMANENT_EXCLUSIONS: false,
          PERMANENT_EXCLUSIONS_RATE: false,
          SESSIONS_ALL: false,
          SESSIONS_AUTHORISED_RATE: false,
          SESSIONS_OVERALL_RATE: false,
          SESSIONS_UNAUTHORISED_RATE: false,
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

  private handleFilterCheckboxChange: ChangeEventHandler<
    HTMLInputElement
  > = event => {
    let filters: Filters;

    const checkedValue = event.target.checked;

    switch (event.target.value) {
      case 'GENERAL_ALL':
        filters = {
          ...this.state.filters,
          GENERAL_ENROLMENTS: checkedValue,
          GENERAL_SCHOOLS: checkedValue,
        };
        break;
      case 'SESSIONS_ALL':
        filters = {
          ...this.state.filters,
          SESSIONS_AUTHORISED_RATE: checkedValue,
          SESSIONS_OVERALL_RATE: checkedValue,
          SESSIONS_UNAUTHORISED_RATE: checkedValue,
        };
        break;
      case 'EXCLUSIONS_ALL':
        filters = {
          ...this.state.filters,
          FIXED_PERIOD_EXCLUSIONS: checkedValue,
          FIXED_PERIOD_EXCLUSIONS_RATE: checkedValue,
          PERMANENT_EXCLUSIONS: checkedValue,
          PERMANENT_EXCLUSIONS_RATE: checkedValue,
        };
        break;
      default:
        filters = {
          ...this.state.filters,
          [event.target.value]: checkedValue,
        };
    }

    filters = {
      ...filters,
      EXCLUSIONS_ALL:
        filters.PERMANENT_EXCLUSIONS_RATE &&
        filters.PERMANENT_EXCLUSIONS &&
        filters.FIXED_PERIOD_EXCLUSIONS_RATE &&
        filters.FIXED_PERIOD_EXCLUSIONS,
      GENERAL_ALL: filters.GENERAL_ENROLMENTS && filters.GENERAL_SCHOOLS,
      SESSIONS_ALL:
        filters.SESSIONS_UNAUTHORISED_RATE &&
        filters.SESSIONS_OVERALL_RATE &&
        filters.SESSIONS_AUTHORISED_RATE,
    };

    const checkedFilters = Object.entries(filters)
      .filter(([_, isChecked]) => isChecked)
      .map(([key]) => key);

    const tableData = checkedFilters
      .filter(key => allTableData[key])
      .map(key => allTableData[key]);

    this.setState({
      filters,
      tableData,
    });
  };

  private handleRadioChange: RadioChangeEventHandler<{
    value: DataToggles;
  }> = event => {
    this.setState({ dataToggle: event.target.value });
  };

  private hasSessionsFilters() {
    return (
      this.state.filters.SESSIONS_AUTHORISED_RATE ||
      this.state.filters.SESSIONS_OVERALL_RATE ||
      this.state.filters.SESSIONS_UNAUTHORISED_RATE
    );
  }

  private hasGeneralFilters() {
    return (
      this.state.filters.GENERAL_ENROLMENTS ||
      this.state.filters.GENERAL_SCHOOLS
    );
  }

  private hasPermanentExclusionFilters() {
    return (
      this.state.filters.PERMANENT_EXCLUSIONS ||
      this.state.filters.PERMANENT_EXCLUSIONS_RATE
    );
  }

  private hasFixedPeriodExclusionFilters() {
    return (
      this.state.filters.FIXED_PERIOD_EXCLUSIONS ||
      this.state.filters.FIXED_PERIOD_EXCLUSIONS_RATE
    );
  }

  private hasAnyFilters() {
    return Object.entries(this.state.filters).some(
      ([_, isChecked]) => isChecked,
    );
  }

  public render() {
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
                        checkedValues={this.state.filters}
                        name="enrolments"
                        legend="General"
                        onChange={this.handleFilterCheckboxChange}
                        options={[
                          {
                            id: 'general-all',
                            label: 'Select all',
                            value: 'GENERAL_ALL',
                          },
                          {
                            id: 'general-enrolments',
                            label: 'Enrolments',
                            value: 'GENERAL_ENROLMENTS',
                          },
                          {
                            id: 'general-schools',
                            label: 'Schools',
                            value: 'GENERAL_SCHOOLS',
                          },
                        ]}
                      />
                    </div>

                    <div className="govuk-form-group">
                      <FormCheckboxGroup
                        checkedValues={this.state.filters}
                        name="pupilAbsence"
                        legend="Sessions absent"
                        onChange={this.handleFilterCheckboxChange}
                        options={[
                          {
                            id: 'sessions-all',
                            label: 'Select all',
                            value: 'SESSIONS_ALL',
                          },
                          {
                            id: 'sessions-authorised-rate',
                            label: 'Authorised rate',
                            value: 'SESSIONS_AUTHORISED_RATE',
                          },
                          {
                            id: 'sessions-overall-rate',
                            label: 'Overall rate',
                            value: 'SESSIONS_OVERALL_RATE',
                          },
                          {
                            id: 'sessions-unauthorised-rate',
                            label: 'Unauthorised rate',
                            value: 'SESSIONS_UNAUTHORISED_RATE',
                          },
                        ]}
                      />
                    </div>
                  </>
                )}

                {this.state.menuOption === 'EXCLUSIONS' && (
                  <FormCheckboxGroup
                    checkedValues={this.state.filters}
                    name="pupilAbsence"
                    legend="Exclusions"
                    onChange={this.handleFilterCheckboxChange}
                    options={[
                      {
                        id: 'exclusions-all',
                        label: 'Select all',
                        value: 'EXCLUSIONS_ALL',
                      },
                      {
                        id: 'permanent-exclusions',
                        label: 'Permanent exclusions',
                        value: 'PERMANENT_EXCLUSIONS',
                      },
                      {
                        id: 'permanent-exclusions-rate',
                        label: 'Permanent exclusion rate',
                        value: 'PERMANENT_EXCLUSIONS_RATE',
                      },
                      {
                        id: 'fixed-period-exclusions',
                        label: 'Fixed period exclusions',
                        value: 'FIXED_PERIOD_EXCLUSIONS',
                      },
                      {
                        id: 'fixed-period-exclusions-rate',
                        label: 'Fixed period exclusion rate',
                        value: 'FIXED_PERIOD_EXCLUSIONS_RATE',
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
                                            this.state.filters
                                              .GENERAL_ENROLMENTS
                                          }
                                          schools={
                                            this.state.filters.GENERAL_SCHOOLS
                                          }
                                        />
                                      </div>
                                    )}
                                    {this.hasSessionsFilters() && (
                                      <div className="govuk-grid-column-one-half">
                                        <p>Sessions absent</p>

                                        <PrototypeAbsenceRateChart
                                          authorised={
                                            this.state.filters
                                              .SESSIONS_AUTHORISED_RATE
                                          }
                                          unauthorised={
                                            this.state.filters
                                              .SESSIONS_UNAUTHORISED_RATE
                                          }
                                          overall={
                                            this.state.filters
                                              .SESSIONS_OVERALL_RATE
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
                                            this.state.filters
                                              .PERMANENT_EXCLUSIONS
                                          }
                                          exclusionsRate={
                                            this.state.filters
                                              .PERMANENT_EXCLUSIONS_RATE
                                          }
                                        />
                                      </div>
                                    )}
                                    {this.hasFixedPeriodExclusionFilters() && (
                                      <div className="govuk-grid-column-one-half">
                                        <p>Fixed period exclusions</p>

                                        <PrototypeFixedPeriodExclusionsChart
                                          exclusions={
                                            this.state.filters
                                              .FIXED_PERIOD_EXCLUSIONS
                                          }
                                          exclusionsRate={
                                            this.state.filters
                                              .FIXED_PERIOD_EXCLUSIONS_RATE
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
