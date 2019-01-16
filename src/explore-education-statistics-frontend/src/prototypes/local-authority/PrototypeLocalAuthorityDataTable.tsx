import React, { Component } from 'react';
import Button from '../../components/Button';
import FormCheckboxGroup from '../../components/FormCheckboxGroup';
import PageHeading from '../../components/PageHeading';
import Tabs from '../../components/Tabs';
import TabsSection from '../../components/TabsSection';
import PrototypePage from '../components/PrototypePage';
import PrototypeAbsenceRateChart from './charts/PrototypeAbsenceRateChart';
import PrototypeExclusionsChart from './charts/PrototypeExclusionsChart';
import MenuDetails from './components/MenuDetails';
import styles from './PrototypeLocalAuthorityDataTable.module.scss';
import absenceRateData from './test-data/absenceRateData';
import exclusionRateData from './test-data/exclusionRateData';

interface State {
  filters: {
    EXCLUSIONS: boolean;
    PUPIL_ABSENCE: boolean;
  };
}

class PrototypeLocalAuthorityDataTable extends Component<{}, State> {
  public state: State = {
    filters: {
      EXCLUSIONS: true,
      PUPIL_ABSENCE: true,
    },
  };

  private handleChange = (values: { [value: string]: boolean }) => {
    this.setState({
      filters: {
        ...this.state.filters,
        ...values,
      },
    });
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
            <div className={styles.filterMenu}>
              <h3 className="govuk-heading-s">
                Choose publications or indicators
              </h3>

              <form>
                <div className="govuk-form-group">
                  <input type="text" className="govuk-input" />
                </div>

                <Button>Search</Button>
              </form>

              <MenuDetails summary="School statistics (under 16)" open>
                <MenuDetails summary="Absence and exclusions" open>
                  <FormCheckboxGroup
                    name="absenceAndExclusions"
                    onChange={this.handleChange}
                    options={[
                      {
                        checked: true,
                        id: 'pupilAbsence',
                        label: 'Pupil absence',
                        value: 'PUPIL_ABSENCE',
                      },
                      {
                        checked: true,
                        id: 'exclusions',
                        label: 'Exclusions',
                        value: 'EXCLUSIONS',
                      },
                    ]}
                  />
                </MenuDetails>
                <MenuDetails summary="Capacity and admissions" />
                <MenuDetails summary="Results" />
                <MenuDetails summary="School and pupil numbers" />
                <MenuDetails summary="School finance" />
                <MenuDetails summary="Teacher numbers" />
              </MenuDetails>
            </div>
          </div>
          <div className="govuk-grid-column-three-quarters">
            <p>View by:</p>

            <Tabs>
              <TabsSection id="years" title="Years">
                {(this.state.filters.EXCLUSIONS ||
                  this.state.filters.PUPIL_ABSENCE) && (
                  <>
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

                    <hr />
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
                        {this.state.filters.EXCLUSIONS && (
                          <>
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
                              {absenceRateData.map(({ Unauthorised, name }) => (
                                <td key={name}>{`${Unauthorised}%`}</td>
                              ))}
                            </tr>
                          </>
                        )}
                        {this.state.filters.PUPIL_ABSENCE && (
                          <>
                            <tr>
                              <td scope="row">
                                Primary permanent exclusions rate
                              </td>
                              {exclusionRateData.map(({ Primary, name }) => (
                                <td key={name}>{`${Primary.toFixed(3)}%`}</td>
                              ))}
                            </tr>
                            <tr>
                              <td scope="row">
                                Secondary permanent exclusions rate
                              </td>
                              {exclusionRateData.map(({ Secondary, name }) => (
                                <td key={name}>{`${Secondary.toFixed(3)}%`}</td>
                              ))}
                            </tr>
                            <tr>
                              <td scope="row">
                                Special permanent exclusions rate
                              </td>
                              {exclusionRateData.map(({ Special, name }) => (
                                <td key={name}>{`${Special.toFixed(3)}%`}</td>
                              ))}
                            </tr>
                            <tr>
                              <td scope="row">
                                Primary permanent exclusions rate
                              </td>
                              {exclusionRateData.map(({ Total, name }) => (
                                <td key={name}>{`${Total.toFixed(3)}%`}</td>
                              ))}
                            </tr>
                          </>
                        )}
                      </tbody>
                    </table>

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

export default PrototypeLocalAuthorityDataTable;
