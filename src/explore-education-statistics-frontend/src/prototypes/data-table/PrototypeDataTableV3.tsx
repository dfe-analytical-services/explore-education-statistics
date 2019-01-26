import camelCase from 'lodash/camelCase';
import debounce from 'lodash/debounce';
import sortBy from 'lodash/sortBy';
import React, {
  ChangeEvent,
  ChangeEventHandler,
  Component,
  createRef,
  FormEventHandler,
} from 'react';
import Button from '../../components/Button';
import {
  FormCheckboxGroup,
  FormGroup,
  FormTextInput,
} from '../../components/form';
import PageHeading from '../../components/PageHeading';
import Tabs from '../../components/Tabs';
import TabsSection from '../../components/TabsSection';
import { KeysWithType } from '../../types/util';
import PrototypePage from '../components/PrototypePage';
import FilterMenuRadios, {
  MenuChangeEventHandler,
} from './components/FilterMenuRadios';
import MenuDetails from './components/MenuDetails';
import { pupilAbsenceAttributes } from './test-data/pupilAbsenceAttributes';
import { pupilAbsenceCharacteristics } from './test-data/pupilAbsenceCharacteristics';

const mapGroupedData = (groupedData: {
  [group: string]: {
    value: string;
    label: string;
  }[];
}) =>
  Object.entries(groupedData).reduce((allGroups, [group, items]) => {
    return {
      ...allGroups,
      [group]: items.reduce((groupItems, item) => {
        return {
          ...groupItems,
          [item.value]: false,
        };
      }, {}),
    };
  }, {});

interface GroupedOptions {
  [group: string]: {
    [value: string]: boolean;
  };
}

interface FilterErrors {
  attributes?: string;
  characteristics?: string;
}

interface State {
  attributes: GroupedOptions;
  characteristics: GroupedOptions;
  chartData: {
    [key: string]: number;
  }[];
  filterErrors: FilterErrors;
  filterSearch: string;
  filtersSubmitted: boolean;
  publicationName: string;
  tableData: string[][];
}

class PrototypeDataTableV3 extends Component<{}, State> {
  public state: State = {
    attributes: mapGroupedData(pupilAbsenceAttributes),
    characteristics: mapGroupedData(pupilAbsenceCharacteristics),
    chartData: [],
    filterErrors: {},
    filterSearch: '',
    filtersSubmitted: false,
    publicationName: '',
    tableData: [],
  };

  private characteristicsRef = createRef<HTMLDivElement>();
  private dataTableRef = createRef<HTMLDivElement>();

  private handleMenuChange: MenuChangeEventHandler = menuOption => {
    const menuOptionLabels: any = {
      EXCLUSIONS: 'Exclusions',
      PUPIL_ABSENCE: 'Pupil absence',
    };

    this.setState(
      {
        publicationName: menuOptionLabels[menuOption],
      },
      () => {
        if (this.characteristicsRef.current) {
          this.characteristicsRef.current.scrollIntoView({
            behavior: 'smooth',
          });
        }
      },
    );
  };

  // private handleFilterCheckboxChange = (
  //   filterGroupName: 'pupilAbsence' | 'exclusions',
  //   subFilterGroupName: 'exclusions' | 'general' | 'sessions',
  //   event: ChangeEvent<HTMLInputElement>,
  // ) => {
  // const filterGroup = this.state.filters[filterGroupName] as any;
  // const subFilterGroup = filterGroup[subFilterGroupName];
  //
  // let filters: any;
  //
  // if (event.target.value === 'all') {
  //   filters = {
  //     ...this.state.filters,
  //     [filterGroupName]: {
  //       ...filterGroup,
  //       [subFilterGroupName]: Object.keys(subFilterGroup).reduce(
  //         (acc, key) => {
  //           return {
  //             ...acc,
  //             [key]: event.target.checked,
  //           };
  //         },
  //         {},
  //       ),
  //     },
  //   };
  // } else {
  //   filters = {
  //     ...this.state.filters,
  //     [filterGroupName]: {
  //       ...filterGroup,
  //       [subFilterGroupName]: {
  //         ...subFilterGroup,
  //         [event.target.value]: event.target.checked,
  //       },
  //     },
  //   };
  // }
  //
  // const tableData = Object.entries(filters).flatMap(
  //   ([publicationKey, publication]) => {
  //     return Object.entries(publication)
  //       .flatMap(([groupKey, group]) => {
  //         return Object.entries(group)
  //           .filter(([_, isChecked]) => isChecked)
  //           .map(([filterKey]) => {
  //             return allTableData[publicationKey][groupKey][filterKey];
  //           });
  //       })
  //       .filter(row => row.length > 0);
  //   },
  // );
  //
  // this.setState({
  //   filters,
  //   tableData,
  // });
  // };

  private handleSearchChange: ChangeEventHandler<HTMLInputElement> = event => {
    event.persist();
    this.setDebouncedFilterSearch(event);
  };

  private setDebouncedFilterSearch = debounce(
    (event: ChangeEvent<HTMLInputElement>) => {
      this.setState({
        filterSearch: event.target.value,
      });
    },
    300,
  );

  private handleOptionGroupChange = (
    stateKey: KeysWithType<State, GroupedOptions>,
    groupKey: string,
    { target }: ChangeEvent<HTMLInputElement>,
  ) => {
    this.setState({
      ...this.state,
      [stateKey]: {
        ...this.state[stateKey],
        [groupKey]: {
          ...this.state[stateKey][groupKey],
          [target.value]: target.checked,
        },
      },
    });
  };

  private handleOptionGroupAllChange(
    stateKey: KeysWithType<State, GroupedOptions>,
    groupKey: string,
    { target }: ChangeEvent<HTMLInputElement>,
  ) {
    this.setState({
      ...this.state,
      [stateKey]: {
        ...this.state[stateKey],
        [groupKey]: {
          ...Object.keys(this.state[stateKey][groupKey]).reduce(
            (acc, item) => ({
              ...acc,
              [item]: target.checked,
            }),
            {},
          ),
        },
      },
    });
  }

  private handleFilterSubmit: FormEventHandler = event => {
    event.preventDefault();

    const hasCheckedFilters = (groupedOptions: GroupedOptions) =>
      Object.entries(groupedOptions).some(([_, groupedOption]) =>
        Object.values(groupedOption).some(Boolean),
      );

    const filterErrors: FilterErrors = {};

    if (!hasCheckedFilters(this.state.attributes)) {
      filterErrors.attributes = 'Select some options below';
    }

    if (!hasCheckedFilters(this.state.characteristics)) {
      filterErrors.characteristics = 'Select some options below';
    }

    this.setState({
      filterErrors,
    });

    if (Object.keys(filterErrors).length > 0) {
      return;
    }

    this.setState(
      {
        filtersSubmitted: true,
      },
      () => {
        if (this.dataTableRef.current) {
          this.dataTableRef.current.scrollIntoView({ behavior: 'smooth' });
        }
      },
    );
  };

  private renderGroupedOptions(
    groupData: {
      [group: string]: {
        value: string;
        label: string;
      }[];
    },
    stateKey: KeysWithType<State, GroupedOptions>,
  ) {
    const containSearchTerm = (value: string) =>
      value.search(new RegExp(this.state.filterSearch, 'i')) > -1;

    const groups = Object.entries(groupData)
      .filter(
        ([groupKey]) =>
          this.state.filterSearch === '' ||
          groupData[groupKey].some(
            item =>
              containSearchTerm(item.label) ||
              this.state[stateKey][groupKey][item.value],
          ),
      )
      .map(([groupKey, items]) => {
        const isMenuOpen = groupData[groupKey].some(
          item =>
            (this.state.filterSearch !== '' && containSearchTerm(item.label)) ||
            this.state[stateKey][groupKey][item.value],
        );

        const options = sortBy(
          items
            .filter(
              item =>
                this.state.filterSearch === '' ||
                containSearchTerm(item.label) ||
                this.state[stateKey][groupKey][item.value],
            )
            .map(item => {
              return {
                ...item,
                id: item.value,
              };
            }),
          ['label'],
        );

        return (
          <MenuDetails summary={groupKey} key={groupKey} open={isMenuOpen}>
            <FormCheckboxGroup
              checkedValues={this.state[stateKey][groupKey]}
              name={`${stateKey}-${camelCase(groupKey)}`}
              onAllChange={
                this.state.filterSearch === ''
                  ? this.handleOptionGroupAllChange.bind(
                      this,
                      stateKey,
                      groupKey,
                    )
                  : undefined
              }
              onChange={this.handleOptionGroupChange.bind(
                this,
                stateKey,
                groupKey,
              )}
              options={options}
            />
          </MenuDetails>
        );
      });

    return groups.length > 0
      ? groups
      : `No options matching '${this.state.filterSearch}'.`;
  }

  public render() {
    const {
      filterErrors,
      filtersSubmitted,
      publicationName,
      tableData,
    } = this.state;

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

        {publicationName && (
          <div className="govuk-grid-row" ref={this.characteristicsRef}>
            <div className="govuk-grid-column-full">
              <h2>
                2. Filter statistics from '{publicationName}'
                <span className="govuk-hint">
                  Select any options you are interested in from the checkboxes
                  below.
                </span>
              </h2>

              <Tabs>
                <TabsSection id="characteristics" title="Characteristics">
                  <form onSubmit={this.handleFilterSubmit}>
                    <FormGroup>
                      <FormTextInput
                        id="characteristic-search"
                        label="Search for a characteristic or attribute"
                        name="characteristicSearch"
                        onChange={this.handleSearchChange}
                        width={20}
                      />
                    </FormGroup>

                    <div className="govuk-grid-row">
                      <div className="govuk-grid-column-one-half">
                        <FormGroup
                          hasError={filterErrors.attributes !== undefined}
                        >
                          <h3>Attributes</h3>

                          {filterErrors.attributes && (
                            <span className="govuk-error-message">
                              {filterErrors.attributes}
                            </span>
                          )}

                          {this.renderGroupedOptions(
                            pupilAbsenceAttributes,
                            'attributes',
                          )}
                        </FormGroup>
                      </div>
                      <div className="govuk-grid-column-one-half">
                        <FormGroup
                          hasError={filterErrors.characteristics !== undefined}
                        >
                          <h3>Characteristics</h3>

                          {filterErrors.characteristics && (
                            <span className="govuk-error-message">
                              {filterErrors.characteristics}
                            </span>
                          )}

                          {this.renderGroupedOptions(
                            pupilAbsenceCharacteristics,
                            'characteristics',
                          )}
                        </FormGroup>
                      </div>
                    </div>

                    <Button type="submit">Confirm filters</Button>
                  </form>
                </TabsSection>
              </Tabs>
            </div>
          </div>
        )}

        {filtersSubmitted && (
          <div ref={this.dataTableRef}>
            <h2>3. Explore statistics from '{publicationName}'</h2>

            <table className="govuk-table">
              <caption>Comparing statistics between 2012 and 2017</caption>
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
                {tableData.map(([firstCell, ...cells], rowIndex) => (
                  <tr key={rowIndex}>
                    <td scope="row">{firstCell}</td>
                    {cells.map((cell, cellIndex) => (
                      <td key={cellIndex}>{cell}</td>
                    ))}
                  </tr>
                ))}
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
          </div>
        )}
      </PrototypePage>
    );
  }
}

export default PrototypeDataTableV3;
