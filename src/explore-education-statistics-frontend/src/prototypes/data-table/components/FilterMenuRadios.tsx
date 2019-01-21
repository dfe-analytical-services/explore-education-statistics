import React, { Component, ReactNode } from 'react';
import { RadioChangeEventHandler } from '../../../components/FormRadio';
import FormRadioGroup from '../../../components/FormRadioGroup';
import styles from './FilterMenuRadios.module.scss';
import MenuDetails from './MenuDetails';

export type MenuOption = 'EXCLUSIONS' | 'PUPIL_ABSENCE' | '';

export type MenuChangeEventHandler = (option: MenuOption) => void;

interface Props {
  beforeMenu?: ReactNode;
  onChange: MenuChangeEventHandler;
}

interface State {
  menuOption: MenuOption;
}

class FilterMenuRadios extends Component<Props, State> {
  public state: State = {
    menuOption: '',
  };

  private handleRadioChange: RadioChangeEventHandler<{
    value: MenuOption;
  }> = event => {
    this.setState(
      {
        menuOption: event.target.value,
      },
      () => {
        this.props.onChange(this.state.menuOption);
      },
    );
  };

  public render() {
    return (
      <div className={styles.filterMenu}>
        {this.props.beforeMenu}

        <h2>
          Choose a publication
          <div className="govuk-hint">
            Pick a publication below to explore its statistics
          </div>
        </h2>

        <MenuDetails summary="Schools (under 16)" open>
          <MenuDetails summary="Absence and exclusions" open>
            <FormRadioGroup
              checkedValue={this.state.menuOption}
              name="absenceAndExclusions"
              onChange={this.handleRadioChange}
              options={[
                {
                  id: 'pupilAbsence',
                  label: 'Pupil absence',
                  value: 'PUPIL_ABSENCE',
                },
                {
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
        <MenuDetails summary="16+ education">
          <MenuDetails summary="Absence and exclusions" />
          <MenuDetails summary="Capacity and admissions" />
          <MenuDetails summary="Results" />
          <MenuDetails summary="School and pupil numbers" />
          <MenuDetails summary="School finance" />
          <MenuDetails summary="Teacher numbers" />
        </MenuDetails>
        <MenuDetails summary="Social care">
          <MenuDetails summary="Absence and exclusions" />
          <MenuDetails summary="Capacity and admissions" />
          <MenuDetails summary="Results" />
          <MenuDetails summary="School and pupil numbers" />
          <MenuDetails summary="School finance" />
          <MenuDetails summary="Teacher numbers" />
        </MenuDetails>
      </div>
    );
  }
}

export default FilterMenuRadios;
