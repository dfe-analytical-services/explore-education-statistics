import React, { Component, FormEventHandler, ReactNode } from 'react';
import Button from '../../../components/Button';
import FormRadioGroup, {
  RadioGroupChangeEventHandler,
} from '../../../components/FormRadioGroup';
import styles from './FilterMenuRadios.module.scss';
import MenuDetails from './MenuDetails';

export type MenuOption = 'EXCLUSIONS' | 'PUPIL_ABSENCE';

export type MenuSubmitEventHandler = (option: MenuOption) => void;

interface Props {
  beforeMenu?: ReactNode;
  onSubmit: MenuSubmitEventHandler;
}

interface State {
  menuOption: MenuOption;
}

class FilterMenuRadios extends Component<Props, State> {
  private handleRadioChange: RadioGroupChangeEventHandler<MenuOption> = (
    option: MenuOption,
  ) => {
    this.setState({
      menuOption: option,
    });
  };

  private handleSubmit: FormEventHandler = (e) => {
    e.preventDefault();
    this.props.onSubmit(this.state.menuOption);
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

        <form onSubmit={this.handleSubmit}>
          <div className="govuk-form-group">
            <MenuDetails summary="Schools (under 16)" open>
              <MenuDetails summary="Absence and exclusions" open>
                <FormRadioGroup
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

          <Button type="submit">Explore</Button>
        </form>
      </div>
    );
  }
}

export default FilterMenuRadios;
