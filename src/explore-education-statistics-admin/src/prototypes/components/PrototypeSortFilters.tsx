import React from 'react';
import classNames from 'classnames';

interface Props {
  sortOrder: string;
  onSort: (order: string) => void;
}

export const PrototypeMobileSortFilters = ({ sortOrder, onSort }: Props) => {
  return (
    <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
      <label htmlFor="sort" className="govuk-label govuk-!-margin-right-2">
        Sort results
      </label>
      <select
        name="sort"
        id="sort"
        className="govuk-select"
        value={sortOrder}
        onBlur={e => onSort(e.target.value)}
        onChange={e => onSort(e.target.value)}
      >
        <option value="newest">Newest</option>
        <option value="oldest">Oldest</option>
        <option value="alpha">A to Z</option>
      </select>
    </div>
  );
};

const PrototypeSortFilters = ({ sortOrder, onSort }: Props) => {
  return (
    <>
      <div className="govuk-form-group govuk-!-margin-bottom-0">
        <fieldset className="govuk-fieldset">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--s govuk-!-margin-bottom-0">
            Sort results
          </legend>
          <div className="govuk-radios govuk-radios--small  govuk-radios--inline">
            <div className="govuk-radios__item">
              <input
                type="radio"
                className="govuk-radios__input"
                name="sort"
                id="sort-1"
                checked={sortOrder === 'newest'}
                onChange={() => onSort('newest')}
              />
              <label
                className={classNames('govuk-label', 'govuk-radios__label')}
                htmlFor="sort-1"
              >
                Newest
              </label>
            </div>
            <div className="govuk-radios__item">
              <input
                type="radio"
                className="govuk-radios__input"
                name="sort"
                id="sort-2"
                onChange={() => onSort('oldest')}
                checked={sortOrder === 'oldest'}
              />
              <label
                className={classNames('govuk-label', 'govuk-radios__label')}
                htmlFor="sort-2"
              >
                Oldest
              </label>
            </div>
            <div className="govuk-radios__item">
              <input
                type="radio"
                className="govuk-radios__input"
                name="sort"
                id="sort-3"
                onChange={() => onSort('alpha')}
                checked={sortOrder === 'alpha'}
              />
              <label
                className={classNames('govuk-label', 'govuk-radios__label')}
                htmlFor="sort-3"
              >
                A to Z
              </label>
            </div>
          </div>
        </fieldset>
      </div>
    </>
  );
};

export default PrototypeSortFilters;
