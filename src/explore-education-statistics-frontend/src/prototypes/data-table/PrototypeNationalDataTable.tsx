import React, { Component } from 'react';
import { CheckboxGroupChangeEventHandler } from '../../components/FormCheckboxGroup';
import FormRadioGroup from '../../components/FormRadioGroup';
import PageHeading from '../../components/PageHeading';
import Tabs from '../../components/Tabs';
import TabsSection from '../../components/TabsSection';
import PrototypePage from '../components/PrototypePage';
import PrototypeAbsenceRateChart from './charts/PrototypeAbsenceRateChart';
import PrototypeExclusionsChart from './charts/PrototypeExclusionsChart';
import FilterMenu from './components/FilterMenu';
import absenceRateData from './test-data/absenceRateData';
import exclusionRateData from './test-data/exclusionRateData';

type DataToggles = 'CHARTS_TABLES' | 'CHARTS' | 'TABLES';

interface State {
  dataToggle: DataToggles;
  filters: {
    EXCLUSIONS: boolean;
    PUPIL_ABSENCE: boolean;
  };
}

class PrototypeNationalDataTable extends Component<{}, State> {
  public state: State = {
    dataToggle: 'CHARTS_TABLES',
    filters: {
      EXCLUSIONS: true,
      PUPIL_ABSENCE: true,
    },
  };

  private handleCheckboxChange: CheckboxGroupChangeEventHandler = values => {
    this.setState({
      filters: {
        ...this.state.filters,
        ...values,
      },
    });
  };

  private handleRadioChange = (value: DataToggles | null) => {
    if (value) {
      this.setState({ dataToggle: value });
    }
  };

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
            You can explore all the DfE statistics available at national level here.
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

        <h2>Find and compare statistics at national level</h2>

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-quarter">
            <FilterMenu onChange={this.handleCheckboxChange} />
          </div>
          <div className="govuk-grid-column-three-quarters">
            <p>View by:</p>

            <Tabs>
              <TabsSection id="years" title="Years">
                {(this.state.filters.EXCLUSIONS ||
                  this.state.filters.PUPIL_ABSENCE) && (
                  <>
                    <FormRadioGroup
                      inline
                      name="dataToggle"
                      legend="What do you want to see?"
                      legendSize="s"
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

                    {(this.state.dataToggle === 'CHARTS_TABLES' ||
                      this.state.dataToggle === 'CHARTS') && (
                      <div className="govuk-grid-row">
                        {this.state.filters.PUPIL_ABSENCE && (
                          <div className="govuk-grid-column-one-half">
                            <p>Overall absence rate</p>

                            <PrototypeAbsenceRateChart />
                          </div>
                        )}
                        {this.state.filters.EXCLUSIONS && (
                          <div className="govuk-grid-column-one-half">
                            <p>Exclusions: Permanent exclusion rate</p>

                            <PrototypeExclusionsChart />
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
                              <tr>
                                <td scope="row">Overall absence rate</td>
                                {absenceRateData.map(({ Overall, name }) => (
                                  <td key={name}>{`${Overall}%`}</td>
                                ))}
                              </tr>
                              <tr>
                                <td scope="row">Authorised absence rate</td>
                                {absenceRateData.map(({ Authorised, name }) => (
                                  <td key={name}>{`${Authorised}%`}</td>
                                ))}
                              </tr>
                              <tr>
                                <td scope="row">Unauthorised absence rate</td>
                                {absenceRateData.map(
                                  ({ Unauthorised, name }) => (
                                    <td key={name}>{`${Unauthorised}%`}</td>
                                  ),
                                )}
                              </tr>
                            </>
                          )}
                          {this.state.filters.EXCLUSIONS && (
                            <>
                              <tr className="govuk-table__row--striped">
                                <td colSpan={6} scope="rowgroup">
                                  <strong>Exclusions</strong>
                                </td>
                              </tr>
                              <tr>
                                <td scope="row">
                                  Primary permanent exclusion rate
                                </td>
                                {exclusionRateData.map(({ Primary, name }) => (
                                  <td key={name}>{`${Primary.toFixed(3)}%`}</td>
                                ))}
                              </tr>
                              <tr>
                                <td scope="row">
                                  Secondary permanent exclusion rate
                                </td>
                                {exclusionRateData.map(
                                  ({ Secondary, name }) => (
                                    <td key={name}>{`${Secondary.toFixed(
                                      3,
                                    )}%`}</td>
                                  ),
                                )}
                              </tr>
                              <tr>
                                <td scope="row">
                                  Special permanent exclusion rate
                                </td>
                                {exclusionRateData.map(({ Special, name }) => (
                                  <td key={name}>{`${Special.toFixed(3)}%`}</td>
                                ))}
                              </tr>
                              <tr>
                                <td scope="row">
                                  Primary permanent exclusion rate
                                </td>
                                {exclusionRateData.map(({ Total, name }) => (
                                  <td key={name}>{`${Total.toFixed(3)}%`}</td>
                                ))}
                              </tr>
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

export default PrototypeNationalDataTable;
