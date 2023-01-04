/// <summary>
/// Server Implementation of ICrashClient EndPoints
/// </summary>
public sealed class CrashHub : Hub<ICrashClient>
{
    CrashContext _context;

    /// <summary>
    /// Initialize with SqLite DB
    /// </summary>
    /// <param name="context"></param>
    public CrashHub(CrashContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Add Change to SqLite DB and notify other clients
    /// </summary>
    /// <param name="user"></param>
    /// <param name="Change"></param>
    /// <returns></returns>
    public async Task Add(string user, Change Change)
    {
        try
        {
            _context.Changes.Add(Change);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }

        await Clients.Others.Add(user, new Change(Change));
    }

    /// <summary>
    /// Update Item in SqLite DB and notify other clients
    /// </summary>
    /// <param name="user"></param>
    /// <param name="id"></param>
    /// <param name="Change"></param>
    /// <returns></returns>
    public async Task Update(string user, Guid id, Change Change)
    {
        try
        {
            var removeChange = _context.Changes.FirstOrDefault(r => r.Id == id);
            if (removeChange != null)
            {
                _context.Changes.Remove(removeChange);
            }
            _context.Changes.Add(new Change(Change));
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
        await Clients.Others.Update(user, id, Change);
    }

    /// <summary>
    /// Delete Item in SqLite DB and notify other clients
    /// </summary>
    /// <param name="user"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Delete(string user, Guid id)
    {
        try
        {
            var Change = _context.Changes.FirstOrDefault(r => r.Id == id);
            if (Change == null)
                return;
            _context.Changes.Remove(Change);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
        await Clients.Others.Delete(user, id);
    }

    /// <summary>
    /// Unlock Item in SqLite DB and notify other clients
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task Done(string user)
    {
        try
        {
            List<Change> done = new List<Change>();
            foreach (var _change in _context.Changes)
            {
                ChangeAction action = (ChangeAction)_change.Action;
                if (action.HasFlag(ChangeAction.Lock))
                {
                    action ^= ChangeAction.Temporary; // FORCE REMOVE
                    action ^= ChangeAction.Lock; // FORCE REMOVE
                    
                    // Change.Temporary = false;
                    // Change.LockedBy = null;

                    _change.Action = (int)action;

                    done.Add(_change);
                }
            }
            _context.Changes.UpdateRange(done);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
        await Clients.Others.Done(user);

    }

    /// <summary>
    /// Lock Item in SqLite DB and notify other clients
    /// </summary>
    /// <param name="user"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Select(string user, Guid id)
    {
        try
        {
            var modSpec = _context.Changes.FirstOrDefault(r => r.Id == id);
            if (modSpec == null)
                return;

            ChangeAction action = (ChangeAction)modSpec.Action;
            action ^= ChangeAction.Temporary; // FORCE ADD
            action ^= ChangeAction.Lock; // FORCE ADD
                                         // How do we denote locked by?

            modSpec.Action = (int)action;
            // modSpec.Temporary = true;
            // modSpec.LockedBy = user;

            _context.Changes.Update(modSpec);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
        await Clients.Others.Select(user, id);
    }

    /// <summary>
    /// Unlock Item in SqLite DB and notify other clients
    /// </summary>
    /// <param name="user"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task Unselect(string user, Guid id)
    {
        try
        {
            var unSelect = _context.Changes.FirstOrDefault(r => r.Id == id);
            if (unSelect == null)
                return;

            ChangeAction action = (ChangeAction)unSelect.Action;
            action ^= ChangeAction.Temporary; // FORCE REMOVE
            action ^= ChangeAction.Lock; // FORCE REMOVE

            // Change.Temporary = false;
            // Change.LockedBy = null;

            unSelect.Action = (int)action;

            _context.Changes.Update(unSelect);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
        await Clients.Others.Unselect(user, id);
    }

    /// <summary>
    /// On Connected send user Changes from DB
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var Changes = _context.Changes.ToArray();
        await Clients.Caller.Initialize(Changes);
    }
}