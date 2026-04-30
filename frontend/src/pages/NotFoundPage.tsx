import { Link } from 'react-router-dom';
import { FileQuestion } from 'lucide-react';

export function NotFoundPage() {
  return (
    <div className="flex items-center justify-center p-8">
      <div className="bg-white border border-slate-200 rounded-lg shadow-sm p-6 max-w-md w-full text-center">
        <FileQuestion className="w-10 h-10 text-slate-400 mx-auto mb-3" />
        <h1 className="text-lg font-semibold text-slate-900 mb-2">Page not found</h1>
        <p className="text-sm text-slate-500 mb-4">
          The page you're looking for doesn't exist.
        </p>
        <Link
          to="/dashboard"
          className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-md transition-colors"
        >
          Go to Dashboard
        </Link>
      </div>
    </div>
  );
}
