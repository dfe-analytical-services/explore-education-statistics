import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useMounted from '@common/hooks/useMounted';
import React, {
  CSSProperties,
  RefObject,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
} from 'react';

const TOP_MARGIN = 20;
const BOTTOM_MARGIN = 40;

interface Props {
  contentRef: RefObject<HTMLElement>;
  id: string;
  selector?: string;
  sticky?: boolean;
}

type ViewportPosition = 'before' | 'within' | 'after';

const ContentSectionIndex = ({
  contentRef,
  id,
  selector = 'h2, h3, h4, h5',
  sticky,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);

  const [elements, setElements] = useState<HTMLElement[]>([]);
  const [viewportPosition, setViewportPosition] = useState<ViewportPosition>();
  const [initialBounds, setInitialBounds] = useState<DOMRect>();

  const { isMounted } = useMounted(() => {
    if (contentRef.current) {
      const nextElements = Array.from(
        contentRef.current.querySelectorAll(selector),
      ) as HTMLElement[];

      if (!id) {
        throw new Error('Prop `id` must not be empty');
      }

      nextElements.forEach((element, index) => {
        if (!element.id) {
          // eslint-disable-next-line no-param-reassign
          element.id = `${id}-${index + 1}`;
        }
      });

      setElements(nextElements);
    }
  });

  const [handleScroll] = useDebouncedCallback(() => {
    if (!ref.current || !contentRef.current) {
      return;
    }

    const { height: indexHeight } = ref.current.getBoundingClientRect();
    const { top, bottom } = contentRef.current.getBoundingClientRect();

    const isBefore = top > TOP_MARGIN;
    const isAfter = bottom < indexHeight + TOP_MARGIN + BOTTOM_MARGIN;
    const isWithin = !isBefore && !isAfter;

    let nextViewportPosition: ViewportPosition = 'before';

    if (isWithin) {
      nextViewportPosition = 'within';
    } else if (isAfter) {
      nextViewportPosition = 'after';
    }

    if (nextViewportPosition !== viewportPosition) {
      setViewportPosition(nextViewportPosition);
    }
  }, 10);

  useEffect(() => {
    if (sticky) {
      handleScroll();
      window.addEventListener('scroll', handleScroll);
    }

    return () => {
      if (sticky) {
        window.removeEventListener('scroll', handleScroll);
      }
    };
  }, [sticky]);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useLayoutEffect(() => {
    if (ref.current && !initialBounds) {
      // Need to explicitly set heights/widths using the
      // height/width calculated on the initial render.
      setInitialBounds(ref.current?.getBoundingClientRect());
    }
  });

  const { height, width } = initialBounds ?? {};

  const getContainerStyle = (): CSSProperties => {
    switch (viewportPosition) {
      case 'after':
        return {
          height: contentRef.current?.getBoundingClientRect()?.height,
        };
      default:
        return {};
    }
  };

  const getStyle = (): CSSProperties => {
    switch (viewportPosition) {
      case 'within':
        return {
          position: 'fixed',
          top: TOP_MARGIN,
        };
      case 'after':
        return {
          position: 'absolute',
          bottom: BOTTOM_MARGIN,
        };
      default:
        return {};
    }
  };

  if (!isMounted || !elements.length) {
    return null;
  }

  return (
    <div
      className="dfe-print-hidden"
      style={{
        position: 'relative',
        height,
        width,
        ...getContainerStyle(),
      }}
    >
      <div
        ref={ref}
        style={{
          height,
          width,
          ...getStyle(),
        }}
      >
        <h3 className="govuk-heading-s">In this section</h3>

        <ul className="govuk-body-s">
          {elements.map((element, index) => (
            // eslint-disable-next-line react/no-array-index-key
            <li key={index}>
              <a href={`#${element.id}`}>{element.textContent}</a>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};
export default ContentSectionIndex;
