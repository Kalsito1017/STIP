import { render } from '@testing-library/react';
import { Skeleton, SkeletonCard, SkeletonTable, SkeletonChart, SkeletonRankingList, SkeletonPageCard } from './Skeleton';

describe('Skeleton', () => {
  it('renders with animate-pulse class', () => {
    const { container } = render(<Skeleton />);
    expect(container.firstChild).toHaveClass('animate-pulse');
  });

  it('applies custom className', () => {
    const { container } = render(<Skeleton className="h-10 w-20" />);
    expect(container.firstChild).toHaveClass('h-10');
    expect(container.firstChild).toHaveClass('w-20');
  });
});

describe('SkeletonCard', () => {
  it('renders card skeleton structure', () => {
    const { container } = render(<SkeletonCard />);
    const pulses = container.querySelectorAll('.animate-pulse');
    expect(pulses.length).toBeGreaterThanOrEqual(3);
  });
});

describe('SkeletonTable', () => {
  it('renders default 5 rows with 3 columns', () => {
    const { container } = render(<SkeletonTable />);
    const rows = container.querySelectorAll('.border-b');
    // header row + 5 body rows
    expect(rows.length).toBe(6);
  });

  it('renders custom rows and columns', () => {
    const { container } = render(<SkeletonTable rows={2} cols={2} />);
    const rows = container.querySelectorAll('.border-b');
    expect(rows.length).toBe(3);
  });
});

describe('SkeletonChart', () => {
  it('renders with default height', () => {
    const { container } = render(<SkeletonChart />);
    const bars = container.querySelector('.flex.items-end');
    expect(bars).toBeInTheDocument();
  });
});

describe('SkeletonRankingList', () => {
  it('renders default 5 rows', () => {
    const { container } = render(<SkeletonRankingList />);
    const rows = container.firstChild!.childNodes;
    expect(rows.length).toBe(5);
  });

  it('renders custom row count', () => {
    const { container } = render(<SkeletonRankingList rows={3} />);
    const rows = container.firstChild!.childNodes;
    expect(rows.length).toBe(3);
  });

  it('each row has rank badge and bar structure', () => {
    const { container } = render(<SkeletonRankingList rows={1} />);
    const row = container.firstChild!.firstChild as HTMLElement;
    // rank badge, name, bar, score, pct
    expect(row.querySelectorAll('.animate-pulse').length).toBeGreaterThanOrEqual(3);
  });
});

describe('SkeletonPageCard', () => {
  it('renders page skeleton structure', () => {
    const { container } = render(<SkeletonPageCard />);
    const pulses = container.querySelectorAll('.animate-pulse');
    expect(pulses.length).toBeGreaterThanOrEqual(3);
  });
});
