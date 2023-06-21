import classNames from 'classnames';
import React from 'react';

interface Props {
  size?: 'm' | 'l' | 'xl';
  visible?: boolean;
}

const SectionBreak = ({ size = 'm', visible = true }: Props) => (
  <hr
    className={classNames('govuk-section-break', {
      'govuk-section-break--xl': size === 'xl',
      'govuk-section-break--l': size === 'l',
      'govuk-section-break--m': size === 'm',
      'govuk-section-break--visible': visible,
    })}
  />
);

export default SectionBreak;
