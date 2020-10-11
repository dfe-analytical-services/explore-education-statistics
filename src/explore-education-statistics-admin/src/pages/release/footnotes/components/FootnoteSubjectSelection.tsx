import { FootnoteMetaGetters } from '@admin/pages/release/footnotes/utils/generateFootnoteMetaMap';
import { FootnoteSubject } from '@admin/services/footnoteService';
import React, { ReactNode } from 'react';

interface Props {
  subjectId: string;
  subject: FootnoteSubject;
  footnoteMetaGetters: FootnoteMetaGetters;
}

/* eslint-disable react/no-unused-prop-types */
interface Item {
  id: string;
  label: string;
  selected: boolean;
}
/* eslint-enable react/no-unused-prop-types */

interface Selection extends Item {
  indicatorGroups: {
    id: string;
    label: string;
    selected: boolean;
    indicators: Item[];
  }[];
  filters: {
    id: string;
    label: string;
    selected: boolean;
    filterGroups: {
      id: string;
      label: string;
      selected: boolean;
      filterItems: Item[];
    }[];
  }[];
}

const FootnoteSubjectSelection = ({
  subjectId,
  subject,
  footnoteMetaGetters,
}: Props) => {
  const {
    getSubject,
    getIndicatorGroup,
    getIndicator,
    getFilter,
    getFilterGroup,
    getFilterItem,
  } = footnoteMetaGetters;

  const selectedOption = { id: '-1', label: '(All)', selected: false };

  function getIndicatorGroups(): Selection['indicatorGroups'] {
    if (subject.selected) {
      return [{ ...selectedOption, indicators: [] }];
    }
    return Object.entries(subject.indicatorGroups).map(
      ([indicatorGroupId, indicatorGroup]) => {
        return {
          id: indicatorGroupId,
          label: getIndicatorGroup(indicatorGroupId).label,
          selected: indicatorGroup.selected,
          indicators: indicatorGroup.selected
            ? [selectedOption]
            : indicatorGroup.indicators.map(indicatorId => {
                return {
                  id: indicatorId,
                  label: getIndicator(indicatorId).label,
                  selected: true,
                };
              }),
        };
      },
    );
  }

  function getFilters(): Selection['filters'] {
    if (subject.selected) {
      return [{ ...selectedOption, filterGroups: [] }];
    }
    return Object.entries(subject.filters).map(([filterId, filter]) => {
      return {
        id: filterId,
        label: getFilter(filterId).label,
        selected: filter.selected,
        filterGroups: filter.selected
          ? [{ ...selectedOption, filterItems: [] }]
          : Object.entries(filter.filterGroups).map(
              ([filterGroupId, filterGroup]) => {
                return {
                  id: filterGroupId,
                  label: getFilterGroup(filterGroupId).label,
                  selected: filterGroup.selected,
                  filterItems: filterGroup.selected
                    ? [selectedOption]
                    : filterGroup.filterItems.map(filterItemId => {
                        return {
                          id: filterItemId,
                          label: getFilterItem(filterItemId).label,
                          selected: true,
                        };
                      }),
                };
              },
            ),
      };
    });
  }

  const subjectSelect: Selection = {
    id: subjectId,
    label: getSubject(subjectId).label,
    selected: subject.selected,
    indicatorGroups: getIndicatorGroups(),
    filters: getFilters(),
  };

  function renderItem({ id, selected, label }: Item, children?: ReactNode) {
    return (
      <li key={id}>
        {selected ? <strong>{label}</strong> : label}
        {children}
      </li>
    );
  }

  return (
    <tr key={subjectId}>
      <td>{renderItem(subjectSelect)}</td>
      <td>
        <ul className="govuk-!-margin-top-0">
          {subjectSelect.indicatorGroups.map(indicatorGroup => {
            return renderItem(
              indicatorGroup,
              indicatorGroup.indicators ? (
                <ul className="govuk-!-margin-top-0">
                  {indicatorGroup.indicators.map(indicator => {
                    return renderItem(indicator);
                  })}
                </ul>
              ) : null,
            );
          })}
        </ul>
      </td>
      <td>
        <ul className="govuk-!-margin-top-0">
          {subjectSelect.filters.map(filter => {
            return renderItem(
              filter,
              filter.filterGroups ? (
                <ul className="govuk-!-margin-top-0">
                  {filter.filterGroups.map(filterGroup => {
                    return renderItem(
                      filterGroup,
                      filterGroup.filterItems.length ? (
                        <ul>
                          {filterGroup.filterItems.map(filterItem => {
                            return renderItem(filterItem);
                          })}
                        </ul>
                      ) : null,
                    );
                  })}
                </ul>
              ) : null,
            );
          })}
        </ul>
      </td>
    </tr>
  );
};

export default FootnoteSubjectSelection;
