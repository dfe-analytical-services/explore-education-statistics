import React, { useContext } from 'react';
import classNames from 'classnames';
import { LinkProps } from '@admin/components/Link';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';

interface Props extends LinkProps {
  to: string;
  removeOnClick: () => void;
}

const EditableLink = ({
  removeOnClick,
  to: href,
  children,
  unvisited,
  className,
  ...props
}: Props) => {
  const { isEditing } = useContext(EditingContext);

  return !isEditing ? (
    <a
      className={classNames(
        'govuk-link',
        {
          'govuk-link--no-visited-state': unvisited,
        },
        className,
      )}
      href={href}
      {...props}
      rel="noopener noreferrer"
      target="_blank"
    >
      {children}
    </a>
  ) : (
    <div>
      <div>
        <a
          className={classNames(
            'govuk-link',
            {
              'govuk-link--no-visited-state': unvisited,
            },
            className,
          )}
          href={href}
          {...props}
          rel="noopener noreferrer"
          target="_blank"
        >
          {children}
        </a>
      </div>
      <Button
        className="govuk-!-margin-bottom-1"
        variant="secondary"
        onClick={() => removeOnClick()}
      >
        Remove link
      </Button>
    </div>
  );
};

export default EditableLink;
