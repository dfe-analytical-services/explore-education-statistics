import React, { ChangeEventHandler, Component } from 'react';
import Button from '../../components/Button';
import { RadioChangeEventHandler } from '../../components/FormRadio';
import FormRadioGroup from '../../components/FormRadioGroup';
import PageHeading from '../../components/PageHeading';
import Tabs from '../../components/Tabs';
import TabsSection from '../../components/TabsSection';
import PrototypePage from '../components/PrototypePage';
import PrototypeAbsenceRateChart from './charts/PrototypeAbsenceRateChart';
import PrototypePermanentExclusionsChart from './charts/PrototypePermanentExclusionsChart';
import FilterMenu from './components/FilterMenu';
import { sessionsAbsentTableData } from './test-data/absenceRateData';
import { allTableData as exclusionTableData } from './test-data/exclusionRateData';

type DataToggles = 'CHARTS_TABLES' | 'CHARTS' | 'TABLES' | null;

interface State {
  dataToggle: DataToggles;
  filters: {
    EXCLUSIONS: boolean;
    PUPIL_ABSENCE: boolean;
  };
}

class PrototypeDataTableV1LocalAuthority extends Component<{}, State> {
  public state: State = {
    dataToggle: null,
    filters: {
      EXCLUSIONS: true,
      PUPIL_ABSENCE: true,
    },
  };

  private handleCheckboxChange: ChangeEventHandler<
    HTMLInputElement
  > = event => {
    this.setState({
      filters: {
        ...this.state.filters,
        [event.target.value]: event.target.checked,
      },
    });
  };

  private handleRadioChange: RadioChangeEventHandler<{
    value: DataToggles;
  }> = event => {
    this.setState({ dataToggle: event.target.value });
  };

  public render() {
    return (
      <PrototypePage
        breadcrumbs={[
          { text: 'Education training and skills' },
          { text: 'Local authorities' },
          { text: 'Sheffield' },
          { text: 'Explore statistics' },
        ]}
        wide
      >
        <PageHeading caption="Sheffield" heading="Explore statistics" />

        <ul>
          <li>
            You can explore all the DfE statistics available for Sheffield here.
            You can use our step by step guide, or dive straight in.
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

        <h2>Find and compare statistics for Sheffield</h2>

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-quarter">
            <FilterMenu
              filters={this.state.filters}
              onChange={this.handleCheckboxChange}
              beforeMenu={
                <form>
                  <div className="govuk-form-group">
                    <input type="text" className="govuk-input" />
                  </div>

                  <Button>Search</Button>
                </form>
              }
            />
          </div>
          <div className="govuk-grid-column-three-quarters">
            <p>View by:</p>

            <Tabs>
              <TabsSection id="years" title="Academic years">
                {(this.state.filters.EXCLUSIONS ||
                  this.state.filters.PUPIL_ABSENCE) && (
                  <>
                    <FormRadioGroup
                      checkedValue={this.state.dataToggle}
                      inline
                      name="dataToggle"
                      legend="What do you want to see?"
                      legendSize="s"
                      onChange={this.handleRadioChange}
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

                    {(this.state.dataToggle === 'CHARTS_TABLES' ||
                      this.state.dataToggle === 'CHARTS') && (
                      <div className="govuk-grid-row">
                        {this.state.filters.PUPIL_ABSENCE && (
                          <div className="govuk-grid-column-one-half">
                            <p>Overall absence rate</p>

                            <PrototypeAbsenceRateChart
                              authorised={true}
                              overall={true}
                              unauthorised={true}
                            />
                          </div>
                        )}
                        {this.state.filters.EXCLUSIONS && (
                          <div className="govuk-grid-column-one-half">
                            <p>Exclusions: Permanent exclusion rate</p>

                            <PrototypePermanentExclusionsChart
                              exclusions={true}
                              exclusionsRate={true}
                            />
                          </div>
                        )}
                      </div>
                    )}

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
                          {this.state.filters.PUPIL_ABSENCE && (
                            <>
                              <tr className="govuk-table__row--striped">
                                <td colSpan={6} scope="rowgroup">
                                  <strong>Pupil absence</strong>
                                </td>
                              </tr>
                              {Object.values(sessionsAbsentTableData).map(
                                ([firstCell, ...cells], rowIndex) => (
                                  <tr key={rowIndex}>
                                    <td scope="row">{firstCell}</td>
                                    {cells.map((cell, cellIndex) => (
                                      <td key={cellIndex}>{cell}</td>
                                    ))}
                                  </tr>
                                ),
                              )}
                            </>
                          )}
                          {this.state.filters.EXCLUSIONS && (
                            <>
                              <tr className="govuk-table__row--striped">
                                <td colSpan={6} scope="rowgroup">
                                  <strong>Exclusions</strong>
                                </td>
                              </tr>
                              {Object.values(exclusionTableData).map(
                                ([firstCell, ...cells], rowIndex) => (
                                  <tr key={rowIndex}>
                                    <td scope="row">{firstCell}</td>
                                    {cells.map((cell, cellIndex) => (
                                      <td key={cellIndex}>{cell}</td>
                                    ))}
                                  </tr>
                                ),
                              )}
                            </>
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
          </div>
        </div>
      </PrototypePage>
    );
  }
}

export default PrototypeDataTableV1LocalAuthority;
