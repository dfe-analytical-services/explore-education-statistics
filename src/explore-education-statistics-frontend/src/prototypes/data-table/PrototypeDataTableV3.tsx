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
import {
  DataTableResult,
  getCharacteristicsMeta,
  getNationalCharacteristicsData,
  PublicationCharacteristicsMeta,
  SchoolType,
} from '../../services/dataTableService';
import { KeysWithType } from '../../types/util';
import PrototypePage from '../components/PrototypePage';
import CharacteristicsDataTable from './components/CharacteristicsDataTable';
import FilterMenuRadios, {
  MenuChangeEventHandler,
} from './components/FilterMenuRadios';
import MenuDetails from './components/MenuDetails';

interface GroupedCheckboxOptions {
  [group: string]: {
    [value: string]: boolean;
  };
}

interface FilterErrors {
  attributes?: string;
  characteristics?: string;
}

interface State {
  attributes: GroupedCheckboxOptions;
  characteristics: GroupedCheckboxOptions;
  chartData: {
    [key: string]: number;
  }[];
  filterErrors: FilterErrors;
  filterSearch: string;
  filters: {
    attributes: string[];
    characteristics: string[];
  };
  filtersSubmitted: boolean;
  publicationId: string;
  publicationMeta: Pick<
    PublicationCharacteristicsMeta,
    'attributes' | 'characteristics'
  >;
  publicationName: string;
  schoolTypes: SchoolType[];
  tableData: DataTableResult[];
  years: number[];
}

class PrototypeDataTableV3 extends Component<{}, State> {
  public state: State = {
    attributes: {},
    characteristics: {},
    chartData: [],
    filterErrors: {},
    filterSearch: '',
    filters: {
      attributes: [],
      characteristics: [],
    },
    filtersSubmitted: false,
    publicationId: '',
    publicationMeta: {
      attributes: {},
      characteristics: {},
    },
    publicationName: '',
    schoolTypes: [
      SchoolType.Total,
      SchoolType.State_Funded_Primary,
      SchoolType.State_Funded_Secondary,
    ],
    tableData: [],
    years: [201213, 201314, 201415, 201516, 201617],
  };

  private filtersRef = createRef<HTMLDivElement>();
  private dataTableRef = createRef<HTMLDivElement>();

  private getCheckedValues(groupedData: GroupedCheckboxOptions): string[] {
    return Object.entries(groupedData)
      .flatMap(([_, group]) => Object.entries(group))
      .filter(([_, isChecked]) => isChecked)
      .map(([key]) => key);
  }

  private handleMenuChange: MenuChangeEventHandler = async menuOption => {
    if (!menuOption) {
      return;
    }

    const menuOptions = {
      EXCLUSIONS: {
        id: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
        label: 'Exclusions',
      },
      PUPIL_ABSENCE: {
        id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
        label: 'Pupil absence',
      },
    };

    const chosenOption = menuOptions[menuOption];

    const { data } = await getCharacteristicsMeta(chosenOption.id);

    interface GroupedData {
      [group: string]: {
        name: string;
        label: string;
      }[];
    }

    const mapGroupedDataToCheckboxOptions = (groupedData: GroupedData) =>
      Object.entries(groupedData).reduce((allGroups, [group, items]) => {
        return {
          ...allGroups,
          [group]: items.reduce((groupItems, item) => {
            return {
              ...groupItems,
              [item.name]: false,
            };
          }, {}),
        };
      }, {});

    this.setState(
      {
        attributes: mapGroupedDataToCheckboxOptions(data.attributes),
        characteristics: mapGroupedDataToCheckboxOptions(data.characteristics),
        publicationId: chosenOption.id,
        publicationMeta: data,
        publicationName: chosenOption.label,
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
  };

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
    stateKey: KeysWithType<State, GroupedCheckboxOptions>,
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
    stateKey: KeysWithType<State, GroupedCheckboxOptions>,
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

  private handleFilterSubmit: FormEventHandler = async event => {
    event.preventDefault();

    const hasCheckedFilters = (groupedOptions: GroupedCheckboxOptions) =>
      Object.entries(groupedOptions).some(([_, groupedOption]) =>
        Object.values(groupedOption).some(Boolean),
      );

    const filterErrors: FilterErrors = {};

    if (!hasCheckedFilters(this.state.attributes)) {
      filterErrors.attributes = 'Select at least one option';
    }

    if (!hasCheckedFilters(this.state.characteristics)) {
      filterErrors.characteristics = 'Select at least one option';
    }

    this.setState({
      filterErrors,
    });

    if (Object.keys(filterErrors).length > 0) {
      if (this.filtersRef.current) {
        this.filtersRef.current.scrollIntoView({
          behavior: 'smooth',
          block: 'start',
        });
      }
      return;
    }

    const attributes = this.getCheckedValues(this.state.attributes);
    const characteristics = this.getCheckedValues(this.state.characteristics);

    const { data } = await getNationalCharacteristicsData(
      this.state.publicationId,
      attributes,
      characteristics,
      this.state.schoolTypes,
      this.state.years,
    );

    this.setState(
      {
        filters: {
          attributes,
          characteristics,
        },
        filtersSubmitted: true,
        tableData: data.result,
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

  private renderGroupedOptions(
    groupData: {
      [group: string]: {
        name: string;
        label: string;
      }[];
    },
    stateKey: KeysWithType<State, GroupedCheckboxOptions>,
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
              this.state[stateKey][groupKey][item.name],
          ),
      )
      .map(([groupKey, items]) => {
        const isMenuOpen = groupData[groupKey].some(
          item =>
            (this.state.filterSearch !== '' && containSearchTerm(item.label)) ||
            this.state[stateKey][groupKey][item.name],
        );

        const options = sortBy(
          items
            .filter(
              item =>
                this.state.filterSearch === '' ||
                containSearchTerm(item.label) ||
                this.state[stateKey][groupKey][item.name],
            )
            .map(item => {
              return {
                id: item.name,
                label: item.label,
                value: item.name,
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
      filters,
      filtersSubmitted,
      publicationMeta,
      publicationName,
      schoolTypes,
      tableData,
      years,
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
          <div className="govuk-grid-row" ref={this.filtersRef}>
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
                            publicationMeta.attributes,
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
                            publicationMeta.characteristics,
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

            <CharacteristicsDataTable
              attributes={filters.attributes}
              characteristics={filters.characteristics}
              results={tableData}
              schoolTypes={schoolTypes}
              years={years}
            />

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
