using Microsoft.EntityFrameworkCore.Storage;

using DeckZa.Application.Interfaces;
using DeckZa.Application.Interfaces.Repositories;
using DeckZa.Infrastructure.Data;
using DeckZa.Infrastructure.Repositories.Auth;
using DeckZa.Infrastructure.Repositories.Management;

namespace DeckZa.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly DeckZaDbContext _context;

    private IUmUserAppRepository? _umUserApp;
    private IUmUserGroupRepository? _umUserGroup;
    private IUmUserAppFlagRepository? _umUserAppFlag;
    private IUmUserAppSessionRepository? _umUserAppSession;
    private IRefreshTokenRepository? _refreshTokens;
    private IUmGroupAppRepository? _umGroupApp;
    private ITempGroupAppPendingRepository? _tempGroupAppPending;
    private ITempUserAppPendingRepository? _tempUserAppPending;
    private IUmMenuRepository? _menus;
    private IUmGroupMenuRepository? _groupMenus;
    private IParameterStagingRepository? _parameterStaging;
    private IScreenMenuRepository? _screenMenus;
    private IPasswordResetTokenRepository? _passwordResetTokens;
    private IEmailTemplateRepository? _emailTemplates;
    private IEmailOutboxRepository? _emailOutbox;
    private ITbNotificationRepository? _notifications;
    private IRfNotificationTypeRepository? _notificationTypes;
    private IEmailProviderRepository? _emailProviders;
    private IRfDefaultValueRepository? _defaultValues;
    private ITaskRepository? _tasks;
    private ITaskHistoryRepository? _taskHistory;
    private ITaskImportRepository? _taskImports;
    private IExportJobRepository? _exportJobs;

    public UnitOfWork(DeckZaDbContext context) => _context = context;

    public IUmUserAppRepository UmUserApp => _umUserApp ??= new UmUserAppRepository(_context);
    public IUmUserGroupRepository UmUserGroup => _umUserGroup ??= new UmUserGroupRepository(_context);
    public IUmUserAppFlagRepository UmUserAppFlag => _umUserAppFlag ??= new UmUserAppFlagRepository(_context);
    public IUmUserAppSessionRepository UmUserAppSession => _umUserAppSession ??= new UmUserAppSessionRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
    public IUmGroupAppRepository UmGroupApp => _umGroupApp ??= new UmGroupAppRepository(_context);
    public ITempGroupAppPendingRepository TempGroupAppPending => _tempGroupAppPending ??= new TempGroupAppPendingRepository(_context);
    public ITempUserAppPendingRepository TempUserAppPending => _tempUserAppPending ??= new TempUserAppPendingRepository(_context);
    public IUmMenuRepository Menus => _menus ??= new UmMenuRepository(_context);
    public IUmGroupMenuRepository GroupMenus => _groupMenus ??= new UmGroupMenuRepository(_context);
    public IParameterStagingRepository ParameterStaging => _parameterStaging ??= new ParameterStagingRepository(_context);
    public IScreenMenuRepository ScreenMenus => _screenMenus ??= new ScreenMenuRepository(_context);
    public IPasswordResetTokenRepository PasswordResetTokens => _passwordResetTokens ??= new PasswordResetTokenRepository(_context);
    public IEmailTemplateRepository EmailTemplates => _emailTemplates ??= new EmailTemplateRepository(_context);
    public IEmailOutboxRepository EmailOutbox => _emailOutbox ??= new EmailOutboxRepository(_context);
    public ITbNotificationRepository Notifications => _notifications ??= new TbNotificationRepository(_context);
    public IRfNotificationTypeRepository NotificationTypes => _notificationTypes ??= new RfNotificationTypeRepository(_context);
    public IEmailProviderRepository EmailProviders => _emailProviders ??= new EmailProviderRepository(_context);
    public IRfDefaultValueRepository DefaultValues => _defaultValues ??= new RfDefaultValueRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
    public ITaskHistoryRepository TaskHistory => _taskHistory ??= new TaskHistoryRepository(_context);
    public ITaskImportRepository TaskImports => _taskImports ??= new TaskImportRepository(_context);
    public IExportJobRepository ExportJobs => _exportJobs ??= new ExportJobRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => new UnitOfWorkTransaction(await _context.Database.BeginTransactionAsync(cancellationToken));
}

internal sealed class UnitOfWorkTransaction : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction _transaction;

    public UnitOfWorkTransaction(IDbContextTransaction transaction) => _transaction = transaction;

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => _transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => _transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}


