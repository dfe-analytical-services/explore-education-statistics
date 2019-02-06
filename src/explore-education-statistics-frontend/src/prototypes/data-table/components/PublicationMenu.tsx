import React, { Component, ReactNode } from 'react';
import { RadioChangeEventHandler } from '../../../components/form/FormRadio';
import FormRadioGroup from '../../../components/form/FormRadioGroup';
import MenuDetails from './MenuDetails';
import styles from './PublicationMenu.module.scss';

export type MenuOption = 'EXCLUSIONS' | 'PUPIL_ABSENCE' | '';

export type MenuChangeEventHandler = (option: MenuOption) => void;

interface Props {
  beforeMenu?: ReactNode;
  onChange: MenuChangeEventHandler;
}

interface State {
  menuOption: MenuOption;
}

class PublicationMenu extends Component<Props, State> {
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
          1. Choose a publication
          <div className="govuk-hint">
            Pick a publication below to explore its statistics
          </div>
        </h2>

        <MenuDetails summary="Schools (under 16)" open>
          <MenuDetails summary="Absence and exclusions" open>
            <FormRadioGroup
              value={this.state.menuOption}
              name="absenceAndExclusions"
              id="absenceAndExclusions"
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
          <MenuDetails summary="School and pupil numbers" open>
            <FormRadioGroup
              value={this.state.menuOption}
              name="schoolAndPupilNumbers"
              id="schoolAndPupilNumbers"
              onChange={this.handleRadioChange}
              options={[
                {
                  id: 'schoolsPupilsAndTheirCharacteristics',
                  label: 'Schools, pupils and their characteristics',
                  value: 'SCHOOLS_PUPILS_CHARACTERISTICS',
                },
              ]}
            />
          </MenuDetails>
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

export default PublicationMenu;
